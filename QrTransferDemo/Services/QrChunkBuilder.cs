using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using QrTransferDemo.Models;

namespace QrTransferDemo.Services;

public sealed class QrChunkBuilder
{
    private const int MetadataBlockSize = 256;
    private const byte MetadataVersion = 2;

    public IReadOnlyList<QrChunkPacket> BuildPackets(byte fileIndex, string fileName, byte[] fileContent, int chunkSize, string correctionLevel)
    {
        if (fileIndex >= 16)
        {
            throw new ArgumentOutOfRangeException(nameof(fileIndex));
        }

        if (fileContent.Length > ushort.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(fileContent));
        }

        if (chunkSize <= 0 || chunkSize > byte.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(chunkSize));
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name must be provided.", nameof(fileName));
        }

        var frames = new List<QrChunkPacket>();
        var metadata = BuildMetadata(fileName, fileContent, chunkSize, correctionLevel);
        frames.AddRange(SplitIntoPackets(fileIndex, isMetadata: true, metadata, chunkSize));
        frames.AddRange(SplitIntoPackets(fileIndex, isMetadata: false, fileContent, chunkSize));
        return frames;
    }

    private static IEnumerable<QrChunkPacket> SplitIntoPackets(byte fileIndex, bool isMetadata, byte[] source, int chunkSize)
    {
        var totalLength = (ushort)source.Length;

        if (totalLength == 0)
        {
            yield return new QrChunkPacket(fileIndex, isMetadata, 0, 0, Array.Empty<byte>());
            yield break;
        }

        for (var offset = 0; offset < source.Length; offset += chunkSize)
        {
            var remaining = source.Length - offset;
            var currentLength = (byte)Math.Min(chunkSize, remaining);
            var payload = source.AsSpan(offset, currentLength).ToArray();
            yield return new QrChunkPacket(fileIndex, isMetadata, (ushort)offset, totalLength, payload);
        }
    }

    private static byte[] BuildMetadata(string fileName, ReadOnlySpan<byte> fileContent, int chunkSize, string correctionLevel)
    {
        var nameBytes = Encoding.UTF8.GetBytes(fileName);
        if (nameBytes.Length > byte.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(fileName));
        }

        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(MetadataVersion);
            writer.Write((byte)nameBytes.Length);
            writer.Write(nameBytes);
            writer.Write((ushort)fileContent.Length);
            writer.Write((byte)chunkSize);
            writer.Write(MapCorrectionLevel(correctionLevel));
            writer.Write((ushort)MetadataBlockSize);

            var blockCount = (ushort)((fileContent.Length + MetadataBlockSize - 1) / MetadataBlockSize);
            writer.Write(blockCount);

            var fileChecksum = ComputeCrc32(fileContent);
            writer.Write(fileChecksum);

            for (var blockIndex = 0; blockIndex < blockCount; blockIndex++)
            {
                var start = blockIndex * MetadataBlockSize;
                var remaining = fileContent.Length - start;
                var length = Math.Min(MetadataBlockSize, remaining);
                var checksum = ComputeCrc32(fileContent.Slice(start, length));
                writer.Write(checksum);
            }
        }

        return stream.ToArray();
    }

    private static byte MapCorrectionLevel(string correctionLevel)
    {
        if (string.IsNullOrWhiteSpace(correctionLevel))
        {
            return 0;
        }

        var upper = char.ToUpperInvariant(correctionLevel[0]);
        return upper is 'L' or 'M' or 'Q' or 'H' ? (byte)upper : (byte)0;
    }

    private static uint ComputeCrc32(ReadOnlySpan<byte> data)
    {
        uint crc = 0xFFFFFFFFu;
        foreach (var value in data)
        {
            var index = (crc ^ value) & 0xFF;
            crc = (crc >> 8) ^ CrcTable[index];
        }

        return ~crc;
    }

    private static readonly uint[] CrcTable = CreateTable();

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
