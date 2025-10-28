using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using QrTransferDemo.Models;
using QrTransferDemo.Utilities;

namespace QrTransferDemo.Services;

public sealed class QrChunkBuilder
{
    private const int MetadataBlockSize = 256;

    public IReadOnlyList<QrChunkPacket> BuildPackets(
        byte fileIndex,
        string fileName,
        byte[] fileContent,
        int chunkSize,
        string correctionLevel)
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

        var correction = NormalizeCorrectionLevel(correctionLevel);
        var frames = new List<QrChunkPacket>();
        var metadata = BuildMetadata(fileName, fileContent, (byte)chunkSize, correction);
        frames.AddRange(SplitIntoPackets(fileIndex, true, metadata, chunkSize));
        frames.AddRange(SplitIntoPackets(fileIndex, false, fileContent, chunkSize));
        return frames;
    }

    private static byte NormalizeCorrectionLevel(string correctionLevel)
    {
        if (string.IsNullOrEmpty(correctionLevel))
        {
            return 0;
        }

        var trimmed = correctionLevel.Trim();
        if (trimmed.Length == 0)
        {
            return 0;
        }

        if (trimmed.Length != 1)
        {
            throw new ArgumentOutOfRangeException(nameof(correctionLevel));
        }

        return (byte)trimmed[0];
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

    private static byte[] BuildMetadata(string fileName, ReadOnlySpan<byte> fileContent, byte chunkSize, byte correction)
    {
        var nameBytes = Encoding.UTF8.GetBytes(fileName);
        if (nameBytes.Length > byte.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(fileName));
        }

        var fileChecksum = fileContent.Length == 0 ? 0u : Crc32.Compute(fileContent);
        var blockCount = (ushort)((fileContent.Length + MetadataBlockSize - 1) / MetadataBlockSize);
        var blockChecksums = new uint[blockCount];
        for (var blockIndex = 0; blockIndex < blockCount; blockIndex++)
        {
            var start = blockIndex * MetadataBlockSize;
            var remaining = fileContent.Length - start;
            var length = Math.Min(MetadataBlockSize, remaining);
            blockChecksums[blockIndex] = Crc32.Compute(fileContent.Slice(start, length));
        }

        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write((byte)nameBytes.Length);
            writer.Write(nameBytes);
            writer.Write((ushort)fileContent.Length);
            writer.Write(chunkSize);
            writer.Write(correction);
            writer.Write((ushort)MetadataBlockSize);
            writer.Write(blockCount);
            writer.Write(fileChecksum);

            for (var blockIndex = 0; blockIndex < blockChecksums.Length; blockIndex++)
            {
                writer.Write(blockChecksums[blockIndex]);
            }
        }

        return stream.ToArray();
    }
}
