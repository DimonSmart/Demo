namespace Demo.Demos.HashX
{
    using DimonSmart.Hash.Interfaces;
    using Org.BouncyCastle.Crypto.Digests;
    using System;

    public class Md5WebAssemblyAdaptedHashAlgorithm : IHashAlgorithm
    {
        private readonly MD5Digest _md5Digest = new();

        public string Name => "MD5";

        public int HashSize => _md5Digest.GetDigestSize();

        public byte[] ComputeHash(byte[] buffer)
        {
            return ComputeHash(buffer, 0, buffer.Length);
        }

        public byte[] ComputeHash(byte[] buffer, int offset, int count)
        {
            _md5Digest.Reset();
            _md5Digest.BlockUpdate(buffer, offset, count);

            var output = new byte[_md5Digest.GetDigestSize()];
            _md5Digest.DoFinal(output, 0);

            return output;
        }

        public byte[] ComputeHash(ReadOnlySpan<byte> buffer)
        {
            _md5Digest.Reset();

            var byteArray = buffer.ToArray();
            _md5Digest.BlockUpdate(byteArray, 0, byteArray.Length);

            var output = new byte[_md5Digest.GetDigestSize()];
            _md5Digest.DoFinal(output, 0);

            return output;
        }
    }
}
