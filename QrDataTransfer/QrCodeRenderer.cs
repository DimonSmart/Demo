using System;
using QRCoder;

namespace QrDataTransfer;

public sealed class QrCodeRenderer
{
    public string CreateDataUrl(string content, int pixelsPerModule = 8)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("QR content cannot be empty.", nameof(content));
        }

        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(data);
        var bytes = qrCode.GetGraphic(pixelsPerModule);
        return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
    }
}
