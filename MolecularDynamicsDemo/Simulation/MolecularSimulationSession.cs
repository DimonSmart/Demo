namespace MolecularDynamicsDemo.Simulation;

public sealed record MolecularSimulationSession(int SessionId, int ParticleCount, MolecularInitialState InitialState);
