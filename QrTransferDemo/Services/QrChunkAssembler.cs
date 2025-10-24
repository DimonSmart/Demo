using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using QrTransferDemo.Models;

namespace QrTransferDemo.Services;

public sealed class QrChunkAssembler
{
    private const int MaxFileSize = 64 * 1024;

    private readonly ConcurrentDictionary<Guid, FileBuffer> _files = new();

    public ChunkProcessingResult ProcessChunk(QrChunkPacket packet)
    {
        if (packet.ChunkSize <= 0 || string.IsNullOrWhiteSpace(packet.CorrectionLevel))
        {
            return ChunkProcessingResult.InvalidMetadata(packet.FileId);
        }

        if (packet.TotalChunks <= 0 || packet.ChunkIndex < 0 || packet.ChunkIndex >= packet.TotalChunks)
        {
            return ChunkProcessingResult.InvalidMetadata(packet.FileId);
        }

        if (packet.FileSize < 0 || packet.FileSize > MaxFileSize)
        {
            return ChunkProcessingResult.InvalidMetadata(packet.FileId);
        }

        var buffer = _files.GetOrAdd(packet.FileId, id => new FileBuffer(id));
        return buffer.AddChunk(packet);
    }

    public IReadOnlyCollection<FileAssemblySnapshot> GetSnapshots()
    {
        var list = new List<FileAssemblySnapshot>();
        foreach (var entry in _files.Values)
        {
            list.Add(entry.CreateSnapshot());
        }

        return list.AsReadOnly();
    }

    public bool TryGetFile(Guid fileId, out AssembledFile file)
    {
        if (_files.TryGetValue(fileId, out var buffer))
        {
            return buffer.TryGetFile(out file);
        }

        file = null!;
        return false;
    }

    public bool Reset(Guid fileId)
    {
        if (_files.TryGetValue(fileId, out var buffer))
        {
            buffer.Reset();
            return true;
        }

        return false;
    }

    public bool Remove(Guid fileId)
    {
        return _files.TryRemove(fileId, out _);
    }

    private sealed class FileBuffer
    {
        private readonly object _gate = new();
        private readonly Guid _fileId;

        private string? _fileName;
        private long _fileSize;
        private int _chunkSize;
        private string? _correctionLevel;
        private int _totalChunks;
        private uint _fileChecksum;
        private byte[]? _buffer;
        private BitArray? _received;
        private HashSet<int>? _missing;
        private int _receivedCount;
        private long _receivedBytes;
        private int _invalidChunks;
        private int _checksumFailures;
        private byte[]? _assembled;
        private DateTimeOffset _lastUpdated;

        public FileBuffer(Guid fileId)
        {
            _fileId = fileId;
        }

        public ChunkProcessingResult AddChunk(QrChunkPacket packet)
        {
            lock (_gate)
            {
                if (!TryApplyMetadata(packet))
                {
                    return ChunkProcessingResult.InvalidMetadata(_fileId);
                }

                if (_buffer is null || _received is null || _missing is null)
                {
                    return ChunkProcessingResult.InvalidMetadata(_fileId);
                }

                if (_received[packet.ChunkIndex])
                {
                    _lastUpdated = DateTimeOffset.UtcNow;
                    return CreateResult(ChunkAcceptanceStatus.Duplicate, null);
                }

                if (!Crc32Validator.Verify(packet.Payload, packet.PayloadCrc32))
                {
                    _invalidChunks++;
                    _lastUpdated = DateTimeOffset.UtcNow;
                    return CreateResult(ChunkAcceptanceStatus.InvalidCrc, null);
                }

                var startPosition = (long)packet.ChunkIndex * _chunkSize;
                if (startPosition > int.MaxValue)
                {
                    _invalidChunks++;
                    _lastUpdated = DateTimeOffset.UtcNow;
                    return CreateResult(ChunkAcceptanceStatus.InvalidMetadata, null);
                }

                var start = (int)startPosition;
                if (start > _buffer.Length)
                {
                    _invalidChunks++;
                    _lastUpdated = DateTimeOffset.UtcNow;
                    return CreateResult(ChunkAcceptanceStatus.InvalidMetadata, null);
                }

                var payloadLength = packet.Payload.Length;
                if (!ValidatePayloadLength(packet.ChunkIndex, payloadLength))
                {
                    _invalidChunks++;
                    _lastUpdated = DateTimeOffset.UtcNow;
                    return CreateResult(ChunkAcceptanceStatus.InvalidMetadata, null);
                }

                if ((long)start + payloadLength > _buffer.Length)
                {
                    _invalidChunks++;
                    _lastUpdated = DateTimeOffset.UtcNow;
                    return CreateResult(ChunkAcceptanceStatus.InvalidMetadata, null);
                }

                if (payloadLength > 0)
                {
                    packet.Payload.AsSpan().CopyTo(_buffer.AsSpan(start, payloadLength));
                }

                _received[packet.ChunkIndex] = true;
                _missing.Remove(packet.ChunkIndex);
                _receivedCount++;
                _receivedBytes += payloadLength;
                _lastUpdated = DateTimeOffset.UtcNow;

                if (_receivedCount == _totalChunks)
                {
                    return FinalizeFile();
                }

                return CreateResult(ChunkAcceptanceStatus.Accepted, null);
            }
        }

        public FileAssemblySnapshot CreateSnapshot()
        {
            lock (_gate)
            {
            if (_fileName is null || _missing is null)
            {
                return new FileAssemblySnapshot(_fileId, string.Empty, 0, 0, string.Empty, 0, 0, 0, Array.Empty<int>(), 0, 0, _lastUpdated, false, 0);
            }

                return new FileAssemblySnapshot(
                    _fileId,
                    _fileName,
                    _fileSize,
                    _chunkSize,
                    _correctionLevel ?? string.Empty,
                    _totalChunks,
                    _receivedCount,
                    _receivedBytes,
                    _missing.OrderBy(index => index).ToArray(),
                    _invalidChunks,
                    _checksumFailures,
                    _lastUpdated,
                    _assembled is not null,
                    _fileChecksum);
            }
        }

        public bool TryGetFile(out AssembledFile file)
        {
            lock (_gate)
            {
                if (_fileName is not null && _assembled is not null)
                {
                    file = new AssembledFile(
                        _fileId,
                        _fileName,
                        _fileSize,
                        _chunkSize,
                        _correctionLevel ?? string.Empty,
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
            lock (_gate)
            {
                ResetChunks();
                _checksumFailures = 0;
                _invalidChunks = 0;
            }
        }

        private bool TryApplyMetadata(QrChunkPacket packet)
        {
            if (_fileName is null)
            {
                return ApplyMetadata(packet);
            }

            if (!string.Equals(_fileName, packet.FileName, StringComparison.Ordinal) ||
                _fileSize != packet.FileSize ||
                _chunkSize != packet.ChunkSize ||
                _totalChunks != packet.TotalChunks ||
                !string.Equals(_correctionLevel, packet.CorrectionLevel, StringComparison.Ordinal) ||
                _fileChecksum != packet.FileCrc32)
            {
                return ApplyMetadata(packet);
            }

            return _buffer is not null;
        }

        private bool ApplyMetadata(QrChunkPacket packet)
        {
            if (string.IsNullOrWhiteSpace(packet.FileName))
            {
                return false;
            }

            if (packet.FileSize < 0 || packet.FileSize > MaxFileSize)
            {
                return false;
            }

            if (packet.TotalChunks <= 0)
            {
                return false;
            }

            _fileName = packet.FileName;
            _fileSize = packet.FileSize;
            _chunkSize = packet.ChunkSize;
            _correctionLevel = packet.CorrectionLevel;
            _totalChunks = packet.TotalChunks;
            _fileChecksum = packet.FileCrc32;
            _checksumFailures = 0;

            AllocateBuffers();
            return _buffer is not null;
        }

        private bool ValidatePayloadLength(int chunkIndex, int payloadLength)
        {
            if (_fileSize == 0)
            {
                return chunkIndex == 0 && payloadLength == 0;
            }

            var start = (long)chunkIndex * _chunkSize;
            if (chunkIndex == _totalChunks - 1)
            {
                var expected = (int)(_fileSize - start);
                if (expected < 0)
                {
                    return false;
                }

                return payloadLength == expected;
            }

            return payloadLength == _chunkSize;
        }

        private ChunkProcessingResult FinalizeFile()
        {
            var length = (int)_fileSize;
            var data = length == 0 ? Array.Empty<byte>() : new byte[length];
            if (length > 0)
            {
                Array.Copy(_buffer!, 0, data, 0, length);
            }

            if (!Crc32Validator.Verify(data, _fileChecksum))
            {
                _checksumFailures++;
                _assembled = null;
                ResetChunks();
                _lastUpdated = DateTimeOffset.UtcNow;
                return CreateResult(ChunkAcceptanceStatus.InvalidFileChecksum, null);
            }

            _assembled = data;
            _lastUpdated = DateTimeOffset.UtcNow;
            return CreateResult(
                ChunkAcceptanceStatus.Accepted,
                new AssembledFile(
                    _fileId,
                    _fileName ?? string.Empty,
                    _fileSize,
                    _chunkSize,
                    _correctionLevel ?? string.Empty,
                    _fileChecksum,
                    data));
        }

        private ChunkProcessingResult CreateResult(ChunkAcceptanceStatus status, AssembledFile? file)
        {
            var snapshot = new FileAssemblySnapshot(
                _fileId,
                _fileName ?? string.Empty,
                _fileSize,
                _chunkSize,
                _correctionLevel ?? string.Empty,
                _totalChunks,
                _receivedCount,
                _receivedBytes,
                _missing?.OrderBy(index => index).ToArray() ?? Array.Empty<int>(),
                _invalidChunks,
                _checksumFailures,
                _lastUpdated,
                _assembled is not null,
                _fileChecksum);

            return new ChunkProcessingResult(status, snapshot, file);
        }

        private void AllocateBuffers()
        {
            if (_fileSize < 0 || _fileSize > MaxFileSize || _totalChunks <= 0)
            {
                _buffer = null;
                _received = null;
                _missing = null;
                _assembled = null;
                _receivedCount = 0;
                _receivedBytes = 0;
                _invalidChunks = 0;
                _lastUpdated = DateTimeOffset.UtcNow;
                return;
            }

            var capacity = (int)_fileSize;
            _buffer = new byte[capacity];
            _received = new BitArray(_totalChunks);
            _missing = new HashSet<int>(Enumerable.Range(0, _totalChunks));
            _assembled = null;
            _receivedCount = 0;
            _receivedBytes = 0;
            _invalidChunks = 0;
            _lastUpdated = DateTimeOffset.UtcNow;
        }

        private void ResetChunks()
        {
            if (_received is null || _missing is null)
            {
                return;
            }

            _received.SetAll(false);
            _missing.Clear();
            for (var i = 0; i < _totalChunks; i++)
            {
                _missing.Add(i);
            }

            _assembled = null;
            _receivedCount = 0;
            _receivedBytes = 0;
            _lastUpdated = DateTimeOffset.UtcNow;
        }
    }

    private static class Crc32Validator
    {
        private static readonly uint[] Table = CreateTable();

        public static bool Verify(ReadOnlySpan<byte> data, uint expected)
        {
            return Compute(data) == expected;
        }

        private static uint Compute(ReadOnlySpan<byte> data)
        {
            var crc = 0xFFFFFFFFu;
            foreach (var b in data)
            {
                var index = (crc ^ b) & 0xFF;
                crc = (crc >> 8) ^ Table[index];
            }

            return ~crc;
        }

        private static uint[] CreateTable()
        {
            var table = new uint[256];
            const uint polynomial = 0xEDB88320u;

            for (var i = 0; i < table.Length; i++)
            {
                var value = (uint)i;
                for (var j = 0; j < 8; j++)
                {
                    value = (value & 1) != 0 ? polynomial ^ (value >> 1) : value >> 1;
                }

                table[i] = value;
            }

            return table;
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
