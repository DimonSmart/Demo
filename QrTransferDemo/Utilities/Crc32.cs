using System;

namespace QrTransferDemo.Utilities;

public static class Crc32
{
    private static readonly uint[] Table = CreateTable();

    public static uint Compute(ReadOnlySpan<byte> data)
    {
        var crc = 0xFFFFFFFFu;
        foreach (var value in data)
        {
            var index = (crc ^ value) & 0xFF;
            crc = (crc >> 8) ^ Table[index];
        }

        return ~crc;
    }

    private static uint[] CreateTable()
    {
        var table = new uint[256];
        const uint polynomial = 0xEDB88320u;

        for (var i = 0; i < table.Length; i++)
        {
            var value = (uint)i;
            for (var j = 0; j < 8; j++)
            {
                value = (value & 1) != 0 ? polynomial ^ (value >> 1) : value >> 1;
            }

            table[i] = value;
        }

        return table;
    }
}
