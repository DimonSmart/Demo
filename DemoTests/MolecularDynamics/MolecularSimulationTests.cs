using MolecularDynamicsDemo.Simulation;

namespace DemoTests.MolecularDynamics;

public sealed class MolecularSimulationTests
{
    [Fact]
    public void Initialize_FreeGas_CreatesRequestedParticleCountWithoutInitialBonds()
    {
        var simulation = new MolecularSimulation(new Random(1234));
        var session = new MolecularSimulationSession(1, 240, MolecularInitialState.FreeGas);

        simulation.Initialize(session, 900f, 600f);

        Assert.Equal(240, simulation.Statistics.ParticleCount);
        Assert.Equal(0, simulation.Statistics.BondCount);
    }

    [Fact]
    public void Initialize_BondedLayer_CreatesInitialBonds()
    {
        var simulation = new MolecularSimulation(new Random(1234));
        var session = new MolecularSimulationSession(1, 240, MolecularInitialState.BondedLayer);

        simulation.Initialize(session, 900f, 600f);

        Assert.Equal(240, simulation.Statistics.ParticleCount);
        Assert.True(simulation.Statistics.BondCount > 0);
    }

    [Fact]
    public void Initialize_InvalidParticleCount_ThrowsArgumentOutOfRangeException()
    {
        var simulation = new MolecularSimulation(new Random(1234));
        var session = new MolecularSimulationSession(1, 0, MolecularInitialState.FreeGas);

        Assert.Throws<ArgumentOutOfRangeException>(() => simulation.Initialize(session, 900f, 600f));
    }

    [Fact]
    public void Initialize_InvalidViewportSize_ThrowsArgumentOutOfRangeException()
    {
        var simulation = new MolecularSimulation(new Random(1234));
        var session = new MolecularSimulationSession(1, 120, MolecularInitialState.FreeGas);

        Assert.Throws<ArgumentOutOfRangeException>(() => simulation.Initialize(session, float.NaN, 600f));
    }

    [Fact]
    public void MoveParticle_AfterSelectingNearestParticle_MovesThatParticle()
    {
        var simulation = new MolecularSimulation(new Random(1234));
        var session = new MolecularSimulationSession(1, 120, MolecularInitialState.FreeGas);
        simulation.Initialize(session, 900f, 600f);

        var selectedParticleIndex = simulation.FindNearestParticleIndex(450f, 300f, 1000f);

        Assert.NotNull(selectedParticleIndex);

        simulation.MoveParticle(selectedParticleIndex.Value, 100f, 150f, 2f, 3f);

        Assert.Equal(selectedParticleIndex, simulation.FindNearestParticleIndex(100f, 150f, 1f));
    }

    [Fact]
    public void Step_WhenCannonIsFiring_ReturnsCreatedParticleCountAndAddsParticles()
    {
        var simulation = new MolecularSimulation(new Random(1234));
        var session = new MolecularSimulationSession(1, 120, MolecularInitialState.FreeGas);
        simulation.Initialize(session, 900f, 600f);
        simulation.IsCannonFiring = true;
        var emittedParticleCount = 0;

        for (var frame = 0; frame < 12; frame++)
        {
            emittedParticleCount += simulation.Step(1f);
        }

        Assert.Equal(2, emittedParticleCount);
        Assert.Equal(122, simulation.Statistics.ParticleCount);
    }

    [Fact]
    public void Step_WhenPaused_DoesNotEmitCannonParticles()
    {
        var simulation = new MolecularSimulation(new Random(1234));
        var session = new MolecularSimulationSession(1, 120, MolecularInitialState.FreeGas);
        simulation.Initialize(session, 900f, 600f);
        simulation.IsCannonFiring = true;
        simulation.IsPaused = true;
        var emittedParticleCount = 0;

        for (var frame = 0; frame < 12; frame++)
        {
            emittedParticleCount += simulation.Step(1f);
        }

        Assert.Equal(0, emittedParticleCount);
        Assert.Equal(120, simulation.Statistics.ParticleCount);
    }
}
