using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using QrTransferDemo.Models;

namespace QrTransferDemo.Services;

public sealed class QrChunkBuilder
{
    private readonly ConcurrentDictionary<string, Guid> _fileIds = new(StringComparer.Ordinal);
    private static readonly uint[] CrcTable = CreateTable();

    public IReadOnlyList<QrChunkPacket> BuildPackets(string fileName, ReadOnlySpan<byte> fileContent, int chunkSize)
    {
        if (chunkSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(chunkSize));
        }

        var fileId = _fileIds.GetOrAdd(CreateKey(fileName, fileContent.Length), _ => Guid.NewGuid());
        var hash = ComputeFileNameHash(fileName);
        var fileSize = (long)fileContent.Length;

        if (fileContent.Length == 0)
        {
            var crc = ComputeCrc(ReadOnlySpan<byte>.Empty);
            return new[]
            {
                new QrChunkPacket(fileId, hash, fileSize, 0, 1, Array.Empty<byte>(), crc)
            };
        }

        var totalChunks = (int)Math.Ceiling(fileContent.Length / (double)chunkSize);
        var packets = new List<QrChunkPacket>(totalChunks);

        for (var chunkIndex = 0; chunkIndex < totalChunks; chunkIndex++)
        {
            var start = chunkIndex * chunkSize;
            var length = Math.Min(chunkSize, fileContent.Length - start);
            var payload = fileContent.Slice(start, length).ToArray();
            var crc = ComputeCrc(payload);
            packets.Add(new QrChunkPacket(fileId, hash, fileSize, chunkIndex, totalChunks, payload, crc));
        }

        return packets;
    }

    private static string CreateKey(string fileName, int length) => $"{fileName}:{length}";

    private static string ComputeFileNameHash(string fileName)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(fileName));
        return Convert.ToHexString(bytes);
    }

    private static uint ComputeCrc(ReadOnlySpan<byte> data)
    {
        var crc = 0xFFFFFFFFu;
        foreach (var b in data)
        {
            var index = (crc ^ b) & 0xFF;
            crc = (crc >> 8) ^ CrcTable[index];
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
