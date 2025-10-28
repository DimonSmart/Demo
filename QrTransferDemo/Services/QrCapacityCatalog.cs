using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace QrTransferDemo.Services;

public sealed class QrCapacityCatalog
{
    private const int MinVersion = 1;
    private const int MaxVersion = 40;

    private static readonly string[] CorrectionLevels = { "L", "M", "Q", "H" };

    private static readonly int[][] EccCodewordsPerBlock =
    {
        new[] { -1, 7, 10, 15, 20, 26, 18, 20, 24, 30, 18, 20, 24, 26, 30, 22, 24, 28, 30, 28, 28, 28, 28, 30, 30, 26, 28, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30 },
        new[] { -1, 10, 16, 26, 18, 24, 16, 18, 22, 22, 26, 30, 22, 22, 24, 24, 28, 28, 26, 26, 26, 26, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28 },
        new[] { -1, 13, 22, 18, 26, 18, 24, 18, 22, 20, 24, 28, 26, 24, 20, 30, 24, 28, 28, 26, 30, 28, 30, 30, 30, 30, 28, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30 },
        new[] { -1, 17, 28, 22, 16, 22, 28, 26, 26, 24, 28, 24, 28, 22, 24, 24, 30, 28, 28, 26, 28, 30, 24, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30 }
    };

    private static readonly int[][] ErrorCorrectionBlocks =
    {
        new[] { -1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 4, 4, 4, 4, 4, 6, 6, 6, 6, 7, 8, 8, 9, 9, 10, 12, 12, 12, 13, 14, 15, 16, 17, 18, 19, 19, 20, 21, 22, 24, 25 },
        new[] { -1, 1, 1, 1, 2, 2, 4, 4, 4, 5, 5, 5, 8, 9, 9, 10, 10, 11, 13, 14, 16, 17, 17, 18, 20, 21, 23, 25, 26, 28, 29, 31, 33, 35, 37, 38, 40, 43, 45, 47, 49 },
        new[] { -1, 1, 1, 2, 2, 4, 4, 6, 6, 8, 8, 8, 10, 12, 16, 12, 17, 16, 18, 21, 20, 23, 23, 25, 27, 29, 34, 34, 35, 38, 40, 43, 45, 48, 51, 53, 56, 59, 62, 65, 68 },
        new[] { -1, 1, 1, 2, 4, 4, 4, 5, 6, 8, 8, 11, 11, 16, 16, 18, 16, 19, 21, 25, 25, 25, 34, 30, 32, 35, 37, 40, 42, 45, 48, 51, 54, 57, 60, 63, 66, 70, 74, 77, 81 }
    };

    private static readonly IReadOnlyDictionary<int, IReadOnlyDictionary<string, int>> CapacityMatrix = BuildCapacityMatrix();

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

    private static IReadOnlyDictionary<int, IReadOnlyDictionary<string, int>> BuildCapacityMatrix()
    {
        var matrix = new Dictionary<int, IReadOnlyDictionary<string, int>>();
        for (var version = MinVersion; version <= MaxVersion; version++)
        {
            var capacityByLevel = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (var levelIndex = 0; levelIndex < CorrectionLevels.Length; levelIndex++)
            {
                var levelCode = CorrectionLevels[levelIndex];
                capacityByLevel[levelCode] = CalculateCapacity(version, levelIndex);
            }

            matrix[version] = capacityByLevel;
        }

        return matrix;
    }

    private static int CalculateCapacity(int version, int levelIndex)
    {
        var dataCodewords = GetDataCodewords(version, levelIndex);
        var overhead = version <= 9 ? 2 : 3;
        return dataCodewords - overhead;
    }

    private static int GetDataCodewords(int version, int levelIndex)
    {
        var rawDataModules = GetRawDataModules(version);
        var eccCodewords = EccCodewordsPerBlock[levelIndex][version];
        var blockCount = ErrorCorrectionBlocks[levelIndex][version];
        return rawDataModules / 8 - eccCodewords * blockCount;
    }

    private static int GetRawDataModules(int version)
    {
        var size = version * 4 + 17;
        var result = size * size;
        result -= 8 * 8 * 3;
        result -= 15 * 2 + 1;
        result -= (size - 16) * 2;

        if (version >= 2)
        {
            var alignmentCount = version / 7 + 2;
            result -= (alignmentCount - 1) * (alignmentCount - 1) * 25;
            result -= Math.Max(alignmentCount - 2, 0) * 2 * 20;
            if (version >= 7)
            {
                result -= 6 * 3 * 2;
            }
        }

        return result;
    }
}
