namespace QrTransferDemo.Models;

public sealed record QrChunkPacket(
    Guid FileId,
    string FileNameHash,
    long FileSize,
    int ChunkIndex,
    int TotalChunks,
    byte[] Payload,
    uint Crc32);
