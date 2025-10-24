using System.Linq;
using System.Text;
using QrTransferDemo.Services;

namespace DemoTests.QrTransfer;

public class QrChunkAssemblerTests
{
    private readonly QrChunkBuilder _builder = new();

    [Fact]
    public void AssemblesFileWhenAllChunksArrive()
    {
        var assembler = new QrChunkAssembler();
        var content = Encoding.UTF8.GetBytes("Assembler end-to-end test payload.");
        var packets = _builder.BuildPackets("sample.bin", content, 8, "M");

        AssembledFile? completed = null;
        foreach (var packet in packets)
        {
            var result = assembler.ProcessChunk(packet);
            if (result.File is not null)
            {
                completed = result.File;
            }
        }

        Assert.NotNull(completed);
        Assert.Equal(packets[0].FileId, completed!.FileId);
        Assert.Equal("sample.bin", completed.FileName);
        Assert.Equal(packets[0].FileCrc32, completed.FileChecksum);
        Assert.Equal(content, completed.Data);
        Assert.True(assembler.TryGetFile(completed.FileId, out var stored));
        Assert.Equal(content, stored.Data);
        Assert.Equal(completed.FileChecksum, stored.FileChecksum);
    }

    [Fact]
    public void DuplicateChunkDoesNotAdvanceProgress()
    {
        var assembler = new QrChunkAssembler();
        var data = Encoding.UTF8.GetBytes("Duplicate guard");
        var packets = _builder.BuildPackets("dup.bin", data, 4, "M");
        var first = packets.First();

        var firstResult = assembler.ProcessChunk(first);
        var duplicateResult = assembler.ProcessChunk(first);

        Assert.Equal(ChunkAcceptanceStatus.Accepted, firstResult.Status);
        Assert.Equal(1, firstResult.Snapshot.ReceivedChunks);
        Assert.Equal(first.Payload.Length, firstResult.Snapshot.ReceivedBytes);
        Assert.Equal(ChunkAcceptanceStatus.Duplicate, duplicateResult.Status);
        Assert.Equal(1, duplicateResult.Snapshot.ReceivedChunks);
        Assert.Equal(first.Payload.Length, duplicateResult.Snapshot.ReceivedBytes);
    }

    [Fact]
    public void InvalidCrcKeepsChunkPending()
    {
        var assembler = new QrChunkAssembler();
        var data = Encoding.UTF8.GetBytes("CRC check");
        var packet = _builder.BuildPackets("crc.bin", data, 4, "M").First();
        var corrupted = packet with { PayloadCrc32 = packet.PayloadCrc32 + 1 };

        var rejection = assembler.ProcessChunk(corrupted);

        Assert.Equal(ChunkAcceptanceStatus.InvalidCrc, rejection.Status);
        Assert.Equal(0, rejection.Snapshot.ReceivedChunks);
        Assert.Equal(1, rejection.Snapshot.InvalidChunks);
        Assert.Equal(0, rejection.Snapshot.ReceivedBytes);

        var accepted = assembler.ProcessChunk(packet);
        Assert.Equal(ChunkAcceptanceStatus.Accepted, accepted.Status);
        Assert.Equal(1, accepted.Snapshot.ReceivedChunks);
        Assert.Equal(packet.Payload.Length, accepted.Snapshot.ReceivedBytes);
    }

    [Fact]
    public void MetadataChangeResetsBuffer()
    {
        var assembler = new QrChunkAssembler();
        var data = Encoding.UTF8.GetBytes("Metadata toggle");
        var packets = _builder.BuildPackets("meta.bin", data, 4, "M");
        var first = packets.First();

        var initial = assembler.ProcessChunk(first);
        Assert.Equal(ChunkAcceptanceStatus.Accepted, initial.Status);
        Assert.Equal(1, initial.Snapshot.ReceivedChunks);
        Assert.Equal(4, initial.Snapshot.ChunkSize);
        Assert.Equal("M", initial.Snapshot.CorrectionLevel);
        Assert.Equal(first.Payload.Length, initial.Snapshot.ReceivedBytes);

        var altered = first with { CorrectionLevel = "H" };
        var updated = assembler.ProcessChunk(altered);

        Assert.Equal(ChunkAcceptanceStatus.Accepted, updated.Status);
        Assert.Equal("H", updated.Snapshot.CorrectionLevel);
        Assert.Equal(1, updated.Snapshot.ReceivedChunks);
        Assert.Equal(altered.Payload.Length, updated.Snapshot.ReceivedBytes);
    }

    [Fact]
    public void InvalidFileChecksumTriggersRetry()
    {
        var assembler = new QrChunkAssembler();
        var data = Encoding.UTF8.GetBytes("Checksum retry required");
        var packets = _builder.BuildPackets("checksum.bin", data, 5, "L");
        var corruptedMetadata = packets.Select(packet => packet with { FileCrc32 = packet.FileCrc32 + 1 }).ToList();

        ChunkProcessingResult? failure = null;
        foreach (var packet in corruptedMetadata)
        {
            failure = assembler.ProcessChunk(packet);
        }

        Assert.NotNull(failure);
        Assert.Equal(ChunkAcceptanceStatus.InvalidFileChecksum, failure!.Status);
        Assert.Equal(1, failure.Snapshot.ChecksumFailures);
        Assert.Equal(0, failure.Snapshot.ReceivedChunks);
        Assert.Equal(0, failure.Snapshot.ReceivedBytes);

        AssembledFile? completed = null;
        ChunkProcessingResult? finalResult = null;
        foreach (var packet in packets)
        {
            finalResult = assembler.ProcessChunk(packet);
            if (finalResult.File is not null)
            {
                completed = finalResult.File;
            }
        }

        Assert.NotNull(finalResult);
        Assert.Equal(ChunkAcceptanceStatus.Accepted, finalResult!.Status);
        Assert.NotNull(completed);
        Assert.Equal("checksum.bin", completed!.FileName);
        Assert.Equal(packets[0].FileCrc32, completed.FileChecksum);
    }
}
