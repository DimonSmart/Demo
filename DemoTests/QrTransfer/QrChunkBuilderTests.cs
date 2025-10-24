using QrTransferDemo.Models;
using QrTransferDemo.Services;

namespace DemoTests.QrTransfer;

public class QrChunkBuilderTests
{
    [Fact]
    public void BuildPackets_SplitsDataIntoExpectedChunks()
    {
        var builder = new QrChunkBuilder();
        var data = Enumerable.Range(0, 200).Select(i => (byte)i).ToArray();

        var packets = builder.BuildPackets("test.bin", data, 64, "Q");

        Assert.Equal(4, packets.Count);
        Assert.All(packets, packet => Assert.Equal("test.bin", packet.FileName));
        Assert.All(packets, packet => Assert.Equal(data.Length, packet.FileSize));
        Assert.Equal(0, packets[0].ChunkIndex);
        Assert.Equal(3, packets[^1].ChunkIndex);
        Assert.Equal(4, packets[^1].TotalChunks);
        Assert.Equal(64, packets[0].Payload.Length);
        Assert.Equal(64, packets[1].Payload.Length);
        Assert.Equal(64, packets[2].Payload.Length);
        Assert.Equal(8, packets[3].Payload.Length);
        Assert.Equal(64, packets[0].ChunkSize);
        Assert.Equal("Q", packets[0].CorrectionLevel);
    }

    [Fact]
    public void BuildPackets_ReusesFileIdForSameFile()
    {
        var builder = new QrChunkBuilder();
        var data = new byte[16];

        var first = builder.BuildPackets("document.txt", data, 8, "M");
        var second = builder.BuildPackets("document.txt", data, 8, "M");
        var other = builder.BuildPackets("another.txt", data, 8, "M");

        Assert.Equal(first[0].FileId, second[0].FileId);
        Assert.NotEqual(first[0].FileId, other[0].FileId);
    }

    [Fact]
    public void BuildPackets_HandlesEmptyFile()
    {
        var builder = new QrChunkBuilder();

        var packets = builder.BuildPackets("empty.bin", Array.Empty<byte>(), 16, "H");

        Assert.Single(packets);
        var packet = packets[0];
        Assert.Equal(0, packet.ChunkIndex);
        Assert.Equal(1, packet.TotalChunks);
        Assert.Empty(packet.Payload);
        Assert.Equal(0L, packet.FileSize);
        Assert.Equal(0u, packet.PayloadCrc32);
        Assert.Equal(0u, packet.FileCrc32);
        Assert.Equal(16, packet.ChunkSize);
        Assert.Equal("H", packet.CorrectionLevel);
    }
}
