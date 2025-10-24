namespace QrTransferDemo.Models;

public sealed record QrChunkPacket(
    Guid FileId,
    string FileName,
    long FileSize,
    int ChunkSize,
    string CorrectionLevel,
    int ChunkIndex,
    int TotalChunks,
    byte[] Payload,
    uint PayloadCrc32,
    uint FileCrc32)
{
}
