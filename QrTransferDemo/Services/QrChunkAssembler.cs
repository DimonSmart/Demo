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

        if (packet.Payload.Length > byte.MaxValue)
        {
            return ChunkProcessingResult.InvalidMetadata(Guid.Empty);
        }

        var buffer = _files.GetOrAdd(packet.FileIndex, index => new FileBuffer(index));
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
        private readonly byte _fileIndex;
        private readonly object _sync = new();

        private Guid _fileId = Guid.NewGuid();
        private byte[]? _metadataBuffer;
        private bool[]? _metadataReceived;
        private int _metadataBytesReceived;
        private bool _metadataParsed;

        private string _fileName = string.Empty;
        private int _fileSize;
        private int _chunkSize;
        private string _correctionLevel = string.Empty;
        private ushort _blockSize;
        private uint[] _blockChecksums = Array.Empty<uint>();
        private uint _fileChecksum;

        private byte[]? _dataBuffer;
        private BitArray? _dataChunks;
        private HashSet<int> _missingChunks = new();
        private int _totalChunks;
        private int _receivedChunks;
        private long _receivedBytes;
        private int _invalidChunks;
        private int _checksumFailures;
        private byte[]? _assembled;
        private DateTimeOffset _lastUpdated;

        public FileBuffer(byte fileIndex)
        {
            _fileIndex = fileIndex;
        }

        public Guid FileId => _fileId;

        public ChunkProcessingResult AddPacket(QrChunkPacket packet)
        {
            lock (_sync)
            {
                if (packet.IsMetadata)
                {
                    return ProcessMetadata(packet);
                }

                if (!_metadataParsed)
                {
                    return ChunkProcessingResult.InvalidMetadata(_fileId);
                }

                return ProcessData(packet);
            }
        }

        public FileAssemblySnapshot CreateSnapshot()
        {
            lock (_sync)
            {
                return new FileAssemblySnapshot(
                    _fileId,
                    _fileName,
                    _fileSize,
                    _chunkSize,
                    _correctionLevel,
                    _totalChunks,
                    _receivedChunks,
                    _receivedBytes,
                    _missingChunks.OrderBy(i => i).ToArray(),
                    _invalidChunks,
                    _checksumFailures,
                    _lastUpdated,
                    _assembled is not null,
                    _fileChecksum);
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
                if (_dataChunks is null)
                {
                    return;
                }

                _dataChunks.SetAll(false);
                _missingChunks = new HashSet<int>(Enumerable.Range(0, _totalChunks));
                _receivedChunks = 0;
                _receivedBytes = 0;
                _assembled = null;
                _checksumFailures = 0;
                _invalidChunks = 0;
                _lastUpdated = DateTimeOffset.UtcNow;
            }
        }

        private ChunkProcessingResult ProcessMetadata(QrChunkPacket packet)
        {
            if (packet.TotalLength > MaxMetadataSize)
            {
                return ChunkProcessingResult.InvalidMetadata(_fileId);
            }

            if (packet.TotalLength == 0)
            {
                return ChunkProcessingResult.InvalidMetadata(_fileId);
            }

            if (_metadataBuffer is null || _metadataBuffer.Length != packet.TotalLength)
            {
                StartNewFile(packet.TotalLength);
            }

            if (_metadataBuffer is null || _metadataReceived is null)
            {
                return ChunkProcessingResult.InvalidMetadata(_fileId);
            }

            if (packet.Offset + packet.Payload.Length > _metadataBuffer.Length)
            {
                _invalidChunks++;
                _lastUpdated = DateTimeOffset.UtcNow;
                return ChunkProcessingResult.InvalidMetadata(_fileId);
            }

            var newlyReceived = 0;
            for (var i = 0; i < packet.Payload.Length; i++)
            {
                var index = packet.Offset + i;
                if (!_metadataReceived[index])
                {
                    _metadataReceived[index] = true;
                    newlyReceived++;
                }

                _metadataBuffer[index] = packet.Payload[i];
            }

            if (newlyReceived > 0)
            {
                _metadataBytesReceived += newlyReceived;
                _lastUpdated = DateTimeOffset.UtcNow;
            }

            if (_metadataBytesReceived >= _metadataBuffer.Length)
            {
                if (!TryParseMetadata())
                {
                    _invalidChunks++;
                    return ChunkProcessingResult.InvalidMetadata(_fileId);
                }
            }

            return CreateResult(ChunkAcceptanceStatus.Accepted, null);
        }

        private void StartNewFile(int metadataLength)
        {
            _fileId = Guid.NewGuid();
            _metadataBuffer = new byte[metadataLength];
            _metadataReceived = new bool[metadataLength];
            _metadataBytesReceived = 0;
            _metadataParsed = false;
            _fileName = string.Empty;
            _fileSize = 0;
            _chunkSize = 0;
            _correctionLevel = string.Empty;
            _blockSize = 0;
            _blockChecksums = Array.Empty<uint>();
            _fileChecksum = 0;
            _dataBuffer = null;
            _dataChunks = null;
            _missingChunks = new HashSet<int>();
            _totalChunks = 0;
            _receivedChunks = 0;
            _receivedBytes = 0;
            _invalidChunks = 0;
            _checksumFailures = 0;
            _assembled = null;
            _lastUpdated = DateTimeOffset.UtcNow;
        }

        private bool TryParseMetadata()
        {
            if (_metadataBuffer is null)
            {
                return false;
            }

            try
            {
                using var stream = new MemoryStream(_metadataBuffer, writable: false);
                using var reader = new BinaryReader(stream);

                var version = reader.ReadByte();
                if (version < 1 || version > 2)
                {
                    return false;
                }

                var nameLength = reader.ReadByte();
                if (nameLength > 0)
                {
                    var nameBytes = reader.ReadBytes(nameLength);
                    if (nameBytes.Length != nameLength)
                    {
                        return false;
                    }

                    _fileName = Encoding.UTF8.GetString(nameBytes);
                }
                else
                {
                    _fileName = string.Empty;
                }

                _fileSize = reader.ReadUInt16();
                var chunkSize = reader.ReadByte();
                var correction = reader.ReadByte();
                _blockSize = reader.ReadUInt16();
                var blockCount = reader.ReadUInt16();
                _fileChecksum = reader.ReadUInt32();

                if (_fileSize > MaxFileSize)
                {
                    return false;
                }

                if (_fileSize > 0 && chunkSize == 0)
                {
                    return false;
                }

                _chunkSize = chunkSize;
                _correctionLevel = correction == 0 ? string.Empty : ((char)correction).ToString();

                if (_blockSize == 0)
                {
                    _blockSize = 256;
                }

                if (blockCount > 1024)
                {
                    return false;
                }

                var checksums = new uint[blockCount];
                for (var i = 0; i < blockCount; i++)
                {
                    if (reader.BaseStream.Position + sizeof(uint) > reader.BaseStream.Length)
                    {
                        return false;
                    }

                    checksums[i] = reader.ReadUInt32();
                }

                _blockChecksums = checksums;

                _totalChunks = _fileSize == 0 || _chunkSize == 0
                    ? (_fileSize == 0 ? 0 : 1)
                    : (int)Math.Ceiling(_fileSize / (double)_chunkSize);

                _dataBuffer = _fileSize == 0 ? Array.Empty<byte>() : new byte[_fileSize];
                _dataChunks = _totalChunks > 0 ? new BitArray(_totalChunks) : null;
                _missingChunks = _totalChunks > 0 ? new HashSet<int>(Enumerable.Range(0, _totalChunks)) : new HashSet<int>();
                _receivedChunks = 0;
                _receivedBytes = 0;
                _assembled = null;
                _metadataParsed = true;
                _lastUpdated = DateTimeOffset.UtcNow;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private ChunkProcessingResult ProcessData(QrChunkPacket packet)
        {
            if (_dataBuffer is null)
            {
                return ChunkProcessingResult.InvalidMetadata(_fileId);
            }

            if (packet.TotalLength != _fileSize)
            {
                _invalidChunks++;
                _lastUpdated = DateTimeOffset.UtcNow;
                return ChunkProcessingResult.InvalidMetadata(_fileId);
            }

            if (packet.Offset + packet.Payload.Length > _fileSize)
            {
                _invalidChunks++;
                _lastUpdated = DateTimeOffset.UtcNow;
                return ChunkProcessingResult.InvalidMetadata(_fileId);
            }

            if (_fileSize == 0)
            {
                return FinalizeFile();
            }

            if (_chunkSize == 0)
            {
                _invalidChunks++;
                _lastUpdated = DateTimeOffset.UtcNow;
                return ChunkProcessingResult.InvalidMetadata(_fileId);
            }

            var chunkIndex = packet.Offset / _chunkSize;
            if (_dataChunks is null || chunkIndex >= _totalChunks)
            {
                _invalidChunks++;
                _lastUpdated = DateTimeOffset.UtcNow;
                return ChunkProcessingResult.InvalidMetadata(_fileId);
            }

            if (packet.Offset % _chunkSize != 0 && chunkIndex != _totalChunks - 1)
            {
                _invalidChunks++;
                _lastUpdated = DateTimeOffset.UtcNow;
                return ChunkProcessingResult.InvalidMetadata(_fileId);
            }

            if (_dataChunks[chunkIndex])
            {
                _lastUpdated = DateTimeOffset.UtcNow;
                return CreateResult(ChunkAcceptanceStatus.Duplicate, null);
            }

            var expectedLength = chunkIndex == _totalChunks - 1
                ? _fileSize - packet.Offset
                : _chunkSize;

            if (packet.Payload.Length != expectedLength)
            {
                _invalidChunks++;
                _lastUpdated = DateTimeOffset.UtcNow;
                return ChunkProcessingResult.InvalidMetadata(_fileId);
            }

            packet.Payload.CopyTo(_dataBuffer.AsSpan(packet.Offset, packet.Payload.Length));
            _dataChunks[chunkIndex] = true;
            _missingChunks.Remove(chunkIndex);
            _receivedChunks++;
            _receivedBytes += packet.Payload.Length;
            _lastUpdated = DateTimeOffset.UtcNow;

            if (_receivedChunks >= _totalChunks)
            {
                return FinalizeFile();
            }

            return CreateResult(ChunkAcceptanceStatus.Accepted, null);
        }

        private ChunkProcessingResult FinalizeFile()
        {
            var data = _dataBuffer ?? Array.Empty<byte>();
            if (_fileSize == 0)
            {
                _assembled = Array.Empty<byte>();
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
                        Array.Empty<byte>()));
            }

            if (!VerifyChecksums(data))
            {
                _checksumFailures++;
                ResetDataChunks();
                _lastUpdated = DateTimeOffset.UtcNow;
                return CreateResult(ChunkAcceptanceStatus.InvalidFileChecksum, null);
            }

            var assembled = new byte[_fileSize];
            Array.Copy(data, assembled, _fileSize);
            _assembled = assembled;
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
                    assembled));
        }

        private void ResetDataChunks()
        {
            if (_dataChunks is not null)
            {
                _dataChunks.SetAll(false);
            }

            _missingChunks = _totalChunks > 0 ? new HashSet<int>(Enumerable.Range(0, _totalChunks)) : new HashSet<int>();
            _receivedChunks = 0;
            _receivedBytes = 0;
            _assembled = null;
        }

        private bool VerifyChecksums(ReadOnlySpan<byte> data)
        {
            if (_fileChecksum != 0)
            {
                var crc = Crc32.Compute(data);
                if (crc != _fileChecksum)
                {
                    return false;
                }
            }

            if (_blockChecksums.Length == 0)
            {
                return true;
            }

            for (var blockIndex = 0; blockIndex < _blockChecksums.Length; blockIndex++)
            {
                var start = blockIndex * _blockSize;
                var length = Math.Min(_blockSize, data.Length - start);
                if (length <= 0)
                {
                    break;
                }

                var checksum = Crc32.Compute(data.Slice(start, length));
                if (checksum != _blockChecksums[blockIndex])
                {
                    return false;
                }
            }

            return true;
        }

        private ChunkProcessingResult CreateResult(ChunkAcceptanceStatus status, AssembledFile? file)
        {
            return new ChunkProcessingResult(
                status,
                new FileAssemblySnapshot(
                    _fileId,
                    _fileName,
                    _fileSize,
                    _chunkSize,
                    _correctionLevel,
                    _totalChunks,
                    _receivedChunks,
                    _receivedBytes,
                    _missingChunks.OrderBy(i => i).ToArray(),
                    _invalidChunks,
                    _checksumFailures,
                    _lastUpdated,
                    _assembled is not null,
                    _fileChecksum),
                file);
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
    uint FileChecksum);

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
            new FileAssemblySnapshot(fileId, string.Empty, 0, 0, string.Empty, 0, 0, 0, Array.Empty<int>(), 0, 0, DateTimeOffset.UtcNow, false, 0),
            null);
    }
}
