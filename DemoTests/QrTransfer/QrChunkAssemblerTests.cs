using System.Linq;
using System.Text;
using QrTransferDemo.Models;
using QrTransferDemo.Services;
using QrTransferDemo.Utilities;

namespace DemoTests.QrTransfer;

public class QrChunkAssemblerTests
{
    private readonly QrChunkBuilder _builder = new();

    [Fact]
    public void AssemblesFileWhenAllChunksArrive()
    {
        var assembler = new QrChunkAssembler();
        var content = Encoding.UTF8.GetBytes("Assembler end-to-end test payload.");
        var packets = _builder.BuildPackets(1, "sample.bin", content, 8, "M");

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
        Assert.Equal("sample.bin", completed!.FileName);
        Assert.Equal(content.Length, completed.FileSize);
        Assert.Equal(8, completed.ChunkSize);
        Assert.Equal("M", completed.CorrectionLevel);
        Assert.Equal(content, completed.Data);
        Assert.Equal(Crc32.Compute(content), completed.FileChecksum);
        Assert.True(assembler.TryGetFile(completed.FileId, out var stored));
        Assert.Equal(content, stored.Data);
    }

    [Fact]
    public void DuplicateChunkDoesNotAdvanceProgress()
    {
        var assembler = new QrChunkAssembler();
        var data = Encoding.UTF8.GetBytes("Duplicate guard");
        var packets = _builder.BuildPackets(2, "dup.bin", data, 4, "M");
        var metadata = packets.Where(p => p.IsMetadata);
        var payloads = packets.Where(p => !p.IsMetadata).ToArray();

        foreach (var packet in metadata)
        {
            assembler.ProcessChunk(packet);
        }

        var first = payloads.First();
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
    public void CorruptedDataTriggersRetry()
    {
        var assembler = new QrChunkAssembler();
        var data = Encoding.UTF8.GetBytes("Checksum retry required");
        var packets = _builder.BuildPackets(3, "checksum.bin", data, 5, "L").ToArray();

        // Send metadata as-is
        foreach (var packet in packets.Where(p => p.IsMetadata))
        {
            assembler.ProcessChunk(packet);
        }

        // Corrupt the first data chunk
        var corrupted = packets.First(p => !p.IsMetadata);
        var corruptPayload = corrupted.Payload.ToArray();
        corruptPayload[0] ^= 0xFF;
        var corruptedPacket = new QrChunkPacket(corrupted.FileIndex, corrupted.IsMetadata, corrupted.Offset, corrupted.TotalLength, corruptPayload);

        ChunkProcessingResult? failure = null;
        foreach (var packet in packets.Where(p => !p.IsMetadata))
        {
            failure = assembler.ProcessChunk(packet == corrupted ? corruptedPacket : packet);
        }

        Assert.NotNull(failure);
        Assert.Equal(ChunkAcceptanceStatus.InvalidFileChecksum, failure!.Status);
        Assert.Equal(1, failure.Snapshot.ChecksumFailures);
        Assert.Null(failure.File);

        AssembledFile? completed = null;
        foreach (var packet in packets.Where(p => !p.IsMetadata))
        {
            var result = assembler.ProcessChunk(packet);
            if (result.File is not null)
            {
                completed = result.File;
            }
        }

        Assert.NotNull(completed);
        Assert.Equal(Crc32.Compute(data), completed!.FileChecksum);
    }
}
