namespace MolecularDynamicsDemo.Simulation;

public readonly record struct MolecularSimulationStats(
    int ParticleCount,
    int BondCount,
    float AverageSpeedSquared,
    float FramesPerSecond);
