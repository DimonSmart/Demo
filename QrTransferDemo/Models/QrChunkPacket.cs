namespace QrTransferDemo.Models;

public sealed record QrChunkPacket(
    byte FileIndex,
    bool IsMetadata,
    ushort Offset,
    ushort TotalLength,
    byte[] Payload);
