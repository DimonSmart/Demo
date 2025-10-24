using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using QrTransferDemo.Models;

namespace QrTransferDemo.Services;

public sealed class QrChunkBuilder
{
    private const int MetadataBlockSize = 256;
    private const byte MetadataVersion = 1;

    public IReadOnlyList<QrChunkPacket> BuildPackets(byte fileIndex, string fileName, byte[] fileContent, int chunkSize)
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

        var frames = new List<QrChunkPacket>();
        var metadata = BuildMetadata(fileName, fileContent);
        frames.AddRange(SplitIntoPackets(fileIndex, true, metadata, chunkSize));
        frames.AddRange(SplitIntoPackets(fileIndex, false, fileContent, chunkSize));
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

    private static byte[] BuildMetadata(string fileName, ReadOnlySpan<byte> fileContent)
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
            writer.Write((ushort)MetadataBlockSize);

            var blockCount = (ushort)((fileContent.Length + MetadataBlockSize - 1) / MetadataBlockSize);
            writer.Write(blockCount);

            for (var blockIndex = 0; blockIndex < blockCount; blockIndex++)
            {
                var start = blockIndex * MetadataBlockSize;
                var remaining = fileContent.Length - start;
                var length = Math.Min(MetadataBlockSize, remaining);
                var checksum = ComputeChecksum(fileContent.Slice(start, length));
                writer.Write(checksum);
            }
        }

        return stream.ToArray();
    }

    private static ushort ComputeChecksum(ReadOnlySpan<byte> data)
    {
        uint sum = 0;
        foreach (var value in data)
        {
            sum += value;
        }

        return (ushort)(sum & 0xFFFF);
    }
}
