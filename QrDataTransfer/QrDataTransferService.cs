using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace QrDataTransfer;

public sealed class QrDataTransferService
{
    public const int DefaultMaxPayloadBytes = 512;

    private readonly QrCodeRenderer _renderer;

    public QrDataTransferService(QrCodeRenderer renderer)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
    }

    public IReadOnlyList<QrChunk> CreateChunks(string? data, int maxPayloadBytes = DefaultMaxPayloadBytes)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        if (maxPayloadBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxPayloadBytes), maxPayloadBytes, "Chunk size must be positive.");
        }

        if (data.Length == 0)
        {
            return Array.Empty<QrChunk>();
        }

        var transferId = GenerateTransferId();
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
        var segments = Split(base64, maxPayloadBytes);
        var total = segments.Count;
        var result = new List<QrChunk>(total);

        for (var index = 0; index < total; index++)
        {
            var chunkIndex = index + 1;
            var encodedText = FormatChunk(transferId, chunkIndex, total, segments[index]);
            var imageUrl = _renderer.CreateDataUrl(encodedText);
            result.Add(new QrChunk(transferId, chunkIndex, total, segments[index], encodedText, imageUrl));
        }

        return result;
    }

    public string Reassemble(IEnumerable<string> encodedChunks)
    {
        if (encodedChunks is null)
        {
            throw new ArgumentNullException(nameof(encodedChunks));
        }

        var parsed = new List<(string TransferId, int Index, int Total, string DataFragment)>();

        foreach (var entry in encodedChunks)
        {
            if (string.IsNullOrWhiteSpace(entry))
            {
                continue;
            }

            parsed.Add(ParseChunk(entry));
        }

        if (parsed.Count == 0)
        {
            return string.Empty;
        }

        parsed.Sort(static (left, right) => left.Index.CompareTo(right.Index));
        var transferId = parsed[0].TransferId;
        var total = parsed[0].Total;

        foreach (var chunk in parsed)
        {
            if (!string.Equals(chunk.TransferId, transferId, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Chunks belong to different transfers.");
            }

            if (chunk.Total != total)
            {
                throw new InvalidOperationException("Chunks report different total counts.");
            }
        }

        if (parsed.Count != total)
        {
            throw new InvalidOperationException("Not all chunks are present.");
        }

        var builder = new StringBuilder(parsed.Count * DefaultMaxPayloadBytes);
        foreach (var chunk in parsed)
        {
            builder.Append(chunk.DataFragment);
        }

        var buffer = Convert.FromBase64String(builder.ToString());
        return Encoding.UTF8.GetString(buffer);
    }

    public bool TryParseChunk(string encodedText, out (string TransferId, int Index, int Total, string DataFragment) chunk)
    {
        try
        {
            chunk = ParseChunk(encodedText);
            return true;
        }
        catch
        {
            chunk = default;
            return false;
        }
    }

    private static (string TransferId, int Index, int Total, string DataFragment) ParseChunk(string encodedText)
    {
        if (string.IsNullOrWhiteSpace(encodedText))
        {
            throw new FormatException("Chunk text cannot be empty.");
        }

        var trimmed = encodedText.Trim();
        var pipeIndex = trimmed.IndexOf('|');
        if (pipeIndex <= 0)
        {
            throw new FormatException("Chunk header is missing transfer identifier.");
        }

        var transferId = trimmed[..pipeIndex];
        if (transferId.Length < 4)
        {
            throw new FormatException("Transfer identifier is invalid.");
        }

        var remainder = trimmed[(pipeIndex + 1)..];
        var colonIndex = remainder.IndexOf(':');
        if (colonIndex <= 0)
        {
            throw new FormatException("Chunk header is missing payload separator.");
        }

        var header = remainder[..colonIndex];
        var payload = remainder[(colonIndex + 1)..];
        if (payload.Length == 0)
        {
            throw new FormatException("Chunk payload is empty.");
        }

        var slashIndex = header.IndexOf('/');
        if (slashIndex <= 0)
        {
            throw new FormatException("Chunk header is missing total count.");
        }

        if (!int.TryParse(header[..slashIndex], NumberStyles.None, CultureInfo.InvariantCulture, out var index) || index <= 0)
        {
            throw new FormatException("Chunk index is invalid.");
        }

        if (!int.TryParse(header[(slashIndex + 1)..], NumberStyles.None, CultureInfo.InvariantCulture, out var total) || total <= 0)
        {
            throw new FormatException("Chunk total is invalid.");
        }

        return (transferId, index, total, payload);
    }

    private static string FormatChunk(string transferId, int index, int total, string payload)
        => string.Create(
            transferId.Length + payload.Length + 16,
            (transferId, index, total, payload),
            static (span, value) =>
            {
                var (transferId, index, total, payload) = value;
                transferId.AsSpan().CopyTo(span);
                span[transferId.Length] = '|';
                var position = transferId.Length + 1;
                position += WriteInt(span[position..], index);
                span[position] = '/';
                position += 1;
                position += WriteInt(span[position..], total);
                span[position] = ':';
                position += 1;
                payload.AsSpan().CopyTo(span[position..]);
            });

    private static int WriteInt(Span<char> destination, int value)
    {
        if (!value.TryFormat(destination, out var written, provider: CultureInfo.InvariantCulture))
        {
            throw new InvalidOperationException("Failed to format integer value.");
        }

        return written;
    }

    private static List<string> Split(string value, int chunkSize)
    {
        var list = new List<string>((value.Length + chunkSize - 1) / chunkSize);
        for (var offset = 0; offset < value.Length; offset += chunkSize)
        {
            var length = Math.Min(chunkSize, value.Length - offset);
            list.Add(value.Substring(offset, length));
        }

        return list;
    }

    private static string GenerateTransferId()
    {
        Span<byte> buffer = stackalloc byte[5];
        RandomNumberGenerator.Fill(buffer);
        return Convert.ToHexString(buffer);
    }
}
