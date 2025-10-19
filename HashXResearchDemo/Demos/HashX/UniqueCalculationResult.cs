namespace HashXResearchDemo.Demos.HashX;

public record UniqueCalculationResult(
    string AlgorithmName,
    int BlocksHashed,
    int UniqueHashes,
    int BufferSize,
    int HashLength);
