using DimonSmart.Hash.Interfaces;
using Microsoft.JSInterop;

namespace HashXResearchDemo.Demos.HashX;

public class JsMd5Algorithm : IHashAlgorithm
{
    private readonly IJSInProcessRuntime _jsRuntime;

    public JsMd5Algorithm(IJSRuntime jsRuntime)
    {
        _jsRuntime = (IJSInProcessRuntime)jsRuntime;
    }

    public string Name => "MD5 (JS based)";

    public int HashSize => 16;

    public byte[] ComputeHash(byte[] buffer)
    {
        return ComputeHash(buffer, 0, buffer.Length);
    }

    public byte[] ComputeHash(byte[] buffer, int offset, int count)
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));
        if (offset < 0 || count < 0 || offset + count > buffer.Length)
            throw new ArgumentOutOfRangeException();

        var subBuffer = new byte[count];
        Array.Copy(buffer, offset, subBuffer, 0, count);

        var hexHash = _jsRuntime.Invoke<string>("computeMD5Bytes", subBuffer);

        return HexStringToByteArray(hexHash);
    }

    public byte[] ComputeHash(ReadOnlySpan<byte> buffer)
    {
        return ComputeHash(buffer.ToArray(), 0, buffer.Length);
    }

    private static byte[] HexStringToByteArray(string hex)
    {
        if (string.IsNullOrEmpty(hex))
            return Array.Empty<byte>();

        var numberChars = hex.Length;
        var bytes = new byte[numberChars / 2];
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }
        return bytes;
    }
}
