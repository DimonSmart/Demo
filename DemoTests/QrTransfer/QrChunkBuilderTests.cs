using System.IO;
using System.Linq;
using System.Text;
using QrTransferDemo.Models;
using QrTransferDemo.Services;

namespace DemoTests.QrTransfer;

public class QrChunkBuilderTests
{
    [Fact]
    public void BuildPackets_SplitsMetadataAndDataStreams()
    {
        var builder = new QrChunkBuilder();
        var data = Enumerable.Range(0, 200).Select(i => (byte)i).ToArray();

        var packets = builder.BuildPackets(2, "test.bin", data, 64, "Q");

        Assert.NotEmpty(packets);
        Assert.True(packets.All(p => p.FileIndex == 2));

        var metadataPackets = packets.Where(p => p.IsMetadata).ToArray();
        var dataPackets = packets.Where(p => !p.IsMetadata).ToArray();

        Assert.NotEmpty(metadataPackets);
        Assert.NotEmpty(dataPackets);

        Assert.True(metadataPackets.Select(p => p.Offset).SequenceEqual(metadataPackets.Select(p => p.Offset).OrderBy(v => v)));
        Assert.True(dataPackets.Select(p => p.Offset).SequenceEqual(dataPackets.Select(p => p.Offset).OrderBy(v => v)));

        Assert.Equal(data.Length, dataPackets.Last().TotalLength);
        Assert.Equal(64, dataPackets.First().Payload.Length);
        Assert.Equal(8, dataPackets.Last().Payload.Length);
    }

    [Fact]
    public void BuildPackets_EncodesMetadataWithChecksums()
    {
        var builder = new QrChunkBuilder();
        var content = Enumerable.Range(0, 512).Select(i => (byte)(i * 3)).ToArray();

        var packets = builder.BuildPackets(1, "document.txt", content, 96, "M");
        var metadata = packets.Where(p => p.IsMetadata)
            .OrderBy(p => p.Offset)
            .SelectMany(p => p.Payload)
            .ToArray();

        using var stream = new MemoryStream(metadata);
        using var reader = new BinaryReader(stream, Encoding.UTF8);

        // Actual metadata format: nameLength, nameBytes, fileSize, chunkSize, correction, fileChecksum
        var nameLength = reader.ReadByte();
        var nameBytes = reader.ReadBytes(nameLength);
        var fileSize = reader.ReadUInt16();
        var chunkSize = reader.ReadByte();
        var correction = reader.ReadByte();
        var fileChecksum = reader.ReadUInt32();

        Assert.Equal("document.txt", Encoding.UTF8.GetString(nameBytes));
        Assert.Equal(content.Length, fileSize);
        Assert.Equal(96, chunkSize);
        Assert.Equal('M', (char)correction);
        Assert.NotEqual(0u, fileChecksum);
        
        // Verify we read all metadata bytes
        Assert.Equal(metadata.Length, stream.Position);
    }

    [Fact]
    public void BuildPackets_HandlesEmptyFile()
    {
        var builder = new QrChunkBuilder();

        var packets = builder.BuildPackets(0, "empty.bin", Array.Empty<byte>(), 32, "H");
        var metadata = packets.Single(p => p.IsMetadata);
        var data = packets.Single(p => !p.IsMetadata);

        Assert.Equal((ushort)0, data.TotalLength);
        Assert.Empty(data.Payload);
        Assert.Equal((ushort)0, data.Offset);
        Assert.Equal((ushort)metadata.TotalLength, (ushort)metadata.Payload.Length);
    }
}
