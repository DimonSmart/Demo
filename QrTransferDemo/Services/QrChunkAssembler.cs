using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using QrTransferDemo.Models;
using QrTransferDemo.Utilities;

namespace QrTransferDemo.Services;

public sealed class QrChunkAssembler
{
    private const int MaxFileSize = ushort.MaxValue;
    private const int MaxMetadataSize = ushort.MaxValue;

    private readonly ConcurrentDictionary<byte, FileBuffer> _files = new();

    public ChunkProcessingResult ProcessChunk(QrChunkPacket packet)
    {
        if (packet.FileIndex >= 16)
        {
            return ChunkProcessingResult.InvalidMetadata(Guid.Empty);
        }

        var buffer = _files.GetOrAdd(packet.FileIndex, _ => new FileBuffer());
        return buffer.AddPacket(packet);
    }

    public IReadOnlyCollection<FileAssemblySnapshot> GetSnapshots()
    {
        var result = new List<FileAssemblySnapshot>();
        foreach (var buffer in _files.Values)
        {
            result.Add(buffer.CreateSnapshot());
        }

        return result.AsReadOnly();
    }

    public bool TryGetFile(Guid fileId, out AssembledFile file)
    {
        foreach (var buffer in _files.Values)
        {
            if (buffer.TryGetFile(fileId, out file))
            {
                return true;
            }
        }

        file = null!;
        return false;
    }

    public bool Reset(Guid fileId)
    {
        foreach (var buffer in _files.Values)
        {
            if (buffer.FileId == fileId)
            {
                buffer.Reset();
                return true;
            }
        }

        return false;
    }

    public bool Remove(Guid fileId)
    {
        foreach (var (key, buffer) in _files)
        {
            if (buffer.FileId == fileId)
            {
                return _files.TryRemove(key, out _);
            }
        }

        return false;
    }

    private sealed class FileBuffer
    {
        private readonly object _sync = new();
        private readonly ByteStream _metadataStream = new(MaxMetadataSize, trackPrefix: true);
        private readonly ByteStream _dataStream = new(MaxFileSize, trackPrefix: false);

        private Guid _fileId = Guid.NewGuid();
        private bool _metadataParsed;
        private string _fileName = string.Empty;
        private int _fileSize;
        private int _chunkSize;
        private string _correctionLevel = string.Empty;
        private uint _fileChecksum;
        private byte[]? _assembled;
        private int _invalidChunks;
        private int _checksumFailures;
        private DateTimeOffset _lastUpdated = DateTimeOffset.UtcNow;

        public Guid FileId => _fileId;

        public ChunkProcessingResult AddPacket(QrChunkPacket packet)
        {
            lock (_sync)
            {
                var stream = packet.IsMetadata ? _metadataStream : _dataStream;
                var status = stream.Apply(packet.TotalLength, packet.Offset, packet.Payload);

                if (status == StreamApplyStatus.InvalidTotalLength || status == StreamApplyStatus.OutOfRangeOffset)
                {
                    _invalidChunks++;
                    _lastUpdated = DateTimeOffset.UtcNow;
                    return CreateResult(ChunkAcceptanceStatus.InvalidMetadata, null);
                }

                if (status != StreamApplyStatus.DuplicateOrOverlap)
                {
                    _lastUpdated = DateTimeOffset.UtcNow;
                }

                if (packet.IsMetadata)
                {
                    if (!_metadataParsed && _metadataStream.IsComplete)
                    {
                        if (!TryParseMetadata())
                        {
                            _invalidChunks++;
                            return CreateResult(ChunkAcceptanceStatus.InvalidMetadata, null);
                        }

                        if (_metadataParsed && (_fileSize == 0 || _dataStream.IsComplete))
                        {
                            return FinalizeFile();
                        }
                    }

                    return CreateResult(MapStatus(status), null);
                }

                if (_metadataParsed && _dataStream.TotalLength.HasValue && _dataStream.TotalLength.Value != _fileSize)
                {
                    _invalidChunks++;
                    _lastUpdated = DateTimeOffset.UtcNow;
                    return CreateResult(ChunkAcceptanceStatus.InvalidMetadata, null);
                }

                if (_metadataParsed && _dataStream.IsComplete)
                {
                    return FinalizeFile();
                }

                return CreateResult(MapStatus(status), null);
            }
        }

        public FileAssemblySnapshot CreateSnapshot()
        {
            lock (_sync)
            {
                return BuildSnapshot();
            }
        }

        public bool TryGetFile(Guid fileId, out AssembledFile file)
        {
            lock (_sync)
            {
                if (_fileId == fileId && _assembled is not null)
                {
                    file = new AssembledFile(
                        _fileId,
                        _fileName,
                        _fileSize,
                        _chunkSize,
                        _correctionLevel,
                        _fileChecksum,
                        _assembled.ToArray());
                    return true;
                }
            }

            file = null!;
            return false;
        }

        public void Reset()
        {
            lock (_sync)
            {
                _fileId = Guid.NewGuid();
                _metadataStream.ResetReceptionState();
                _dataStream.ResetReceptionState();
                _metadataParsed = false;
                _fileName = string.Empty;
                _fileSize = 0;
                _chunkSize = 0;
                _correctionLevel = string.Empty;
                _fileChecksum = 0;
                _assembled = null;
                _checksumFailures = 0;
                _invalidChunks = 0;
                _lastUpdated = DateTimeOffset.UtcNow;
            }
        }

        private bool TryParseMetadata()
        {
            var metadataLength = _metadataStream.TotalLength ?? 0;
            if (metadataLength == 0)
            {
                return false;
            }

            var buffer = _metadataStream.GetBuffer();
            if (buffer.Length < metadataLength)
            {
                return false;
            }

            try
            {
                using var stream = new MemoryStream(buffer.Slice(0, metadataLength).ToArray(), writable: false);
                using var reader = new BinaryReader(stream, Encoding.UTF8);

                var nameLength = reader.ReadByte();
                var expectedLength = 1 + nameLength + 2 + 1 + 1 + 4;
                if (metadataLength != expectedLength)
                {
                    return false;
                }

                var nameBytes = nameLength > 0 ? reader.ReadBytes(nameLength) : Array.Empty<byte>();
                if (nameBytes.Length != nameLength)
                {
                    return false;
                }

                _fileName = nameLength > 0 ? Encoding.UTF8.GetString(nameBytes) : string.Empty;

                _fileSize = reader.ReadUInt16();
                _chunkSize = reader.ReadByte();
                var correction = reader.ReadByte();
                _correctionLevel = correction == 0 ? string.Empty : ((char)correction).ToString();
                _fileChecksum = reader.ReadUInt32();

                if (_fileSize > MaxFileSize)
                {
                    return false;
                }

                if (_fileSize > 0 && _chunkSize == 0)
                {
                    return false;
                }

                if (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    return false;
                }

                if (!_dataStream.EnsureTotalLength((ushort)_fileSize))
                {
                    return false;
                }

                _metadataParsed = true;
                _lastUpdated = DateTimeOffset.UtcNow;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private ChunkProcessingResult FinalizeFile()
        {
            if (!_dataStream.TotalLength.HasValue || _dataStream.TotalLength.Value != _fileSize)
            {
                _invalidChunks++;
                _lastUpdated = DateTimeOffset.UtcNow;
                return CreateResult(ChunkAcceptanceStatus.InvalidMetadata, null);
            }

            var expectedLength = _fileSize;
            var data = expectedLength == 0
                ? Array.Empty<byte>()
                : _dataStream.GetBuffer().Slice(0, expectedLength).ToArray();

            var crc = Crc32.Compute(data);
            if (crc != _fileChecksum)
            {
                _checksumFailures++;
                _dataStream.ResetReceptionState();
                _assembled = null;
                _lastUpdated = DateTimeOffset.UtcNow;
                return CreateResult(ChunkAcceptanceStatus.InvalidFileChecksum, null);
            }

            _assembled = data;
            _lastUpdated = DateTimeOffset.UtcNow;
            return CreateResult(
                ChunkAcceptanceStatus.Accepted,
                new AssembledFile(
                    _fileId,
                    _fileName,
                    _fileSize,
                    _chunkSize,
                    _correctionLevel,
                    _fileChecksum,
                    data));
        }

        private ChunkProcessingResult CreateResult(ChunkAcceptanceStatus status, AssembledFile? file)
        {
            return new ChunkProcessingResult(status, BuildSnapshot(), file);
        }

        private FileAssemblySnapshot BuildSnapshot()
        {
            var metadataLength = _metadataStream.TotalLength ?? 0;
            var metadataReceived = _metadataStream.ReceivedCount;
            var metadataPrefix = _metadataStream.ContiguousPrefix;

            var totalChunks = _chunkSize > 0 && _fileSize > 0
                ? (int)Math.Ceiling(_fileSize / (double)_chunkSize)
                : 0;
            var missingChunks = CalculateMissingChunks(totalChunks);
            var receivedChunks = totalChunks - missingChunks.Count;

            var isCompleted = _assembled is not null && _dataStream.TotalLength.HasValue && _dataStream.ReceivedCount >= _dataStream.TotalLength.Value;

            return new FileAssemblySnapshot(
                _fileId,
                _fileName,
                _fileSize,
                _chunkSize,
                _correctionLevel,
                totalChunks,
                receivedChunks,
                _dataStream.ReceivedCount,
                missingChunks,
                _invalidChunks,
                _checksumFailures,
                _lastUpdated,
                isCompleted,
                _fileChecksum,
                metadataLength,
                metadataReceived,
                metadataPrefix);
        }

        private IReadOnlyCollection<int> CalculateMissingChunks(int totalChunks)
        {
            if (totalChunks <= 0 || _chunkSize <= 0 || _fileSize <= 0)
            {
                return Array.Empty<int>();
            }

            var bits = _dataStream.ReceivedBits;
            if (bits is null)
            {
                return Array.Empty<int>();
            }

            var missing = new List<int>();
            for (var chunkIndex = 0; chunkIndex < totalChunks; chunkIndex++)
            {
                var start = chunkIndex * _chunkSize;
                var end = Math.Min(start + _chunkSize, _fileSize);
                var complete = true;
                for (var i = start; i < end; i++)
                {
                    if (!bits[i])
                    {
                        complete = false;
                        break;
                    }
                }

                if (!complete)
                {
                    missing.Add(chunkIndex);
                }
            }

            return missing.Count == 0 ? Array.Empty<int>() : missing.ToArray();
        }
    }

    private static ChunkAcceptanceStatus MapStatus(StreamApplyStatus status)
    {
        return status switch
        {
            StreamApplyStatus.DuplicateOrOverlap => ChunkAcceptanceStatus.Duplicate,
            _ => ChunkAcceptanceStatus.Accepted
        };
    }

    private enum StreamApplyStatus
    {
        AcceptedNewData,
        DuplicateOrOverlap,
        InvalidTotalLength,
        OutOfRangeOffset
    }

    private sealed class ByteStream
    {
        private readonly int _maxLength;
        private readonly bool _trackPrefix;

        private ushort? _declaredLength;
        private byte[]? _buffer;
        private BitArray? _receivedBits;
        private int _receivedCount;
        private int _contiguousPrefix;

        public ByteStream(int maxLength, bool trackPrefix)
        {
            _maxLength = maxLength;
            _trackPrefix = trackPrefix;
        }

        public ushort? TotalLength => _declaredLength;

        public bool IsComplete => _declaredLength.HasValue && _receivedCount >= _declaredLength.Value;

        public int ReceivedCount => _receivedCount;

        public int ContiguousPrefix => !_trackPrefix ? 0 : Math.Min(_contiguousPrefix, _declaredLength ?? 0);

        public BitArray? ReceivedBits => _receivedBits;

        public StreamApplyStatus Apply(ushort totalLength, ushort offset, ReadOnlySpan<byte> payload)
        {
            if (!EnsureTotalLength(totalLength))
            {
                return StreamApplyStatus.InvalidTotalLength;
            }

            if (_declaredLength == 0)
            {
                return StreamApplyStatus.AcceptedNewData;
            }

            var declaredLength = _declaredLength!.Value;

            if (offset >= declaredLength)
            {
                return StreamApplyStatus.OutOfRangeOffset;
            }

            var writeLength = Math.Min(payload.Length, declaredLength - offset);
            if (writeLength <= 0)
            {
                return StreamApplyStatus.DuplicateOrOverlap;
            }

            var newBytes = 0;
            for (var i = 0; i < writeLength; i++)
            {
                var index = offset + i;
                if (!_receivedBits![index])
                {
                    _receivedBits[index] = true;
                    newBytes++;
                }

                _buffer![index] = payload[i];
            }

            if (newBytes > 0)
            {
                _receivedCount += newBytes;
                if (_trackPrefix)
                {
                    while (_contiguousPrefix < declaredLength && _receivedBits![_contiguousPrefix])
                    {
                        _contiguousPrefix++;
                    }
                }
            }

            return newBytes > 0 ? StreamApplyStatus.AcceptedNewData : StreamApplyStatus.DuplicateOrOverlap;
        }

        public bool EnsureTotalLength(ushort totalLength)
        {
            if (!_declaredLength.HasValue)
            {
                if (totalLength > _maxLength)
                {
                    return false;
                }

                _declaredLength = totalLength;
                if (totalLength == 0)
                {
                    _buffer = Array.Empty<byte>();
                    _receivedBits = new BitArray(0);
                    _receivedCount = 0;
                    _contiguousPrefix = 0;
                    return true;
                }

                _buffer = new byte[totalLength];
                _receivedBits = new BitArray(totalLength);
                _receivedCount = 0;
                _contiguousPrefix = 0;
                return true;
            }

            return _declaredLength.Value == totalLength;
        }

        public ReadOnlyMemory<byte> GetBuffer()
        {
            return _buffer is null
                ? ReadOnlyMemory<byte>.Empty
                : _buffer.AsMemory(0, _declaredLength ?? _buffer.Length);
        }

        public void ResetReceptionState()
        {
            if (_receivedBits is null)
            {
                return;
            }

            _receivedBits.SetAll(false);
            _receivedCount = 0;
            if (_trackPrefix)
            {
                _contiguousPrefix = 0;
            }
        }

    }
}

public sealed record FileAssemblySnapshot(
    Guid FileId,
    string FileName,
    long FileSize,
    int ChunkSize,
    string CorrectionLevel,
    int TotalChunks,
    int ReceivedChunks,
    long ReceivedBytes,
    IReadOnlyCollection<int> MissingChunks,
    int InvalidChunks,
    int ChecksumFailures,
    DateTimeOffset LastUpdated,
    bool IsCompleted,
    uint FileChecksum,
    int MetadataLength,
    int MetadataReceivedBytes,
    int MetadataContiguousPrefix);

public sealed record AssembledFile(
    Guid FileId,
    string FileName,
    long FileSize,
    int ChunkSize,
    string CorrectionLevel,
    uint FileChecksum,
    byte[] Data);

public enum ChunkAcceptanceStatus
{
    Accepted,
    Duplicate,
    InvalidCrc,
    InvalidMetadata,
    InvalidFileChecksum
}

public sealed record ChunkProcessingResult(ChunkAcceptanceStatus Status, FileAssemblySnapshot Snapshot, AssembledFile? File)
{
    public static ChunkProcessingResult InvalidMetadata(Guid fileId)
    {
        return new ChunkProcessingResult(
            ChunkAcceptanceStatus.InvalidMetadata,
            new FileAssemblySnapshot(
                fileId,
                string.Empty,
                0,
                0,
                string.Empty,
                0,
                0,
                0,
                Array.Empty<int>(),
                0,
                0,
                DateTimeOffset.UtcNow,
                false,
                0,
                0,
                0,
                0),
            null);
    }
}
