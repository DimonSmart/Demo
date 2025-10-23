using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace QrTransferDemo.Services;

public sealed class QrCapacityCatalog
{
    private static readonly IReadOnlyDictionary<int, IReadOnlyDictionary<string, int>> CapacityMatrix =
        new Dictionary<int, IReadOnlyDictionary<string, int>>
        {
            [1] = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["L"] = 17,
                ["M"] = 14,
                ["Q"] = 11,
                ["H"] = 7
            },
            [2] = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["L"] = 32,
                ["M"] = 26,
                ["Q"] = 20,
                ["H"] = 14
            },
            [3] = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["L"] = 53,
                ["M"] = 42,
                ["Q"] = 32,
                ["H"] = 24
            },
            [4] = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["L"] = 78,
                ["M"] = 62,
                ["Q"] = 46,
                ["H"] = 34
            },
            [5] = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["L"] = 106,
                ["M"] = 84,
                ["Q"] = 60,
                ["H"] = 44
            },
            [6] = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["L"] = 134,
                ["M"] = 106,
                ["Q"] = 74,
                ["H"] = 58
            },
            [7] = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["L"] = 154,
                ["M"] = 122,
                ["Q"] = 86,
                ["H"] = 64
            },
            [8] = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["L"] = 192,
                ["M"] = 152,
                ["Q"] = 108,
                ["H"] = 84
            },
            [9] = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["L"] = 230,
                ["M"] = 180,
                ["Q"] = 130,
                ["H"] = 98
            },
            [10] = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["L"] = 271,
                ["M"] = 213,
                ["Q"] = 151,
                ["H"] = 119
            }
        };

    public IReadOnlyList<int> SupportedVersions { get; } =
        new ReadOnlyCollection<int>(CapacityMatrix.Keys.Order().ToList());

    public bool TryGetCapacity(int version, string correctionLevel, out int capacity)
    {
        capacity = 0;
        if (!CapacityMatrix.TryGetValue(version, out var byLevel))
        {
            return false;
        }

        if (!byLevel.TryGetValue(correctionLevel, out capacity))
        {
            return false;
        }

        return true;
    }
}
