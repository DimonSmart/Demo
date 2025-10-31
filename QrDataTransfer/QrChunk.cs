namespace QrDataTransfer;

public sealed record QrChunk(
    string TransferId,
    int Index,
    int Total,
    string DataFragment,
    string EncodedText,
    string ImageDataUrl)
{
    public override string ToString() => EncodedText;
}
