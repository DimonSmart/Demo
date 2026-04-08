using Demo.Abstractions;
using MolecularDynamicsDemo.Simulation;

namespace MolecularDynamicsDemo.Pages;

public partial class MolecularDynamics
{
    private static readonly MolecularInitialState[] AvailableInitialStates =
    [
        MolecularInitialState.FreeGas,
        MolecularInitialState.BondedLayer,
        MolecularInitialState.SolidBlock
    ];

    private MolecularSimulationSession? CurrentSession { get; set; }

    private MolecularSimulationStats CurrentStatistics { get; set; } =
        new(MolecularSimulationConstants.DefaultParticleCount, 0, 0f, 0f);

    private int sessionId;
    private int ParticleCount { get; set; } = MolecularSimulationConstants.DefaultParticleCount;
    private MolecularInitialState SelectedInitialState { get; set; } = MolecularInitialState.BondedLayer;
    private float LeftWallTemperature { get; set; } = MolecularSimulationConstants.DefaultWallTemperature;
    private float RightWallTemperature { get; set; } = MolecularSimulationConstants.DefaultWallTemperature;
    private float CannonAngleDegrees { get; set; } = MolecularSimulationConstants.DefaultCannonAngleDegrees;
    private float CannonPower { get; set; } = MolecularSimulationConstants.DefaultCannonPower;
    private bool IsPaused { get; set; }
    private bool IsCannonFiring { get; set; }

    protected override void OnInitialized()
    {
        PageChrome.SetPage("Molecular dynamics", PageSurfaceMode.Immersive);
    }

    private void StartSimulation()
    {
        ResetLiveControlsToDefaults();
        CurrentSession = new MolecularSimulationSession(++sessionId, ParticleCount, SelectedInitialState);
        CurrentStatistics = new MolecularSimulationStats(ParticleCount, 0, 0f, 0f);
    }

    private void ResetSimulation()
    {
        if (CurrentSession is null)
        {
            return;
        }

        IsPaused = false;
        IsCannonFiring = false;
        CurrentSession = CurrentSession with { SessionId = ++sessionId };
    }

    private void ReturnToSetup()
    {
        IsCannonFiring = false;
        CurrentSession = null;
        PageChrome.SetPage("Molecular dynamics", PageSurfaceMode.Immersive);
    }

    private void TogglePause()
    {
        IsPaused = !IsPaused;
    }

    private Task OnStatisticsChanged(MolecularSimulationStats statistics)
    {
        CurrentStatistics = statistics;
        return InvokeAsync(StateHasChanged);
    }

    private void BeginFiring()
    {
        IsCannonFiring = true;
    }

    private void EndFiring()
    {
        IsCannonFiring = false;
    }

    private void ResetLiveControlsToDefaults()
    {
        LeftWallTemperature = MolecularSimulationConstants.DefaultWallTemperature;
        RightWallTemperature = MolecularSimulationConstants.DefaultWallTemperature;
        CannonAngleDegrees = MolecularSimulationConstants.DefaultCannonAngleDegrees;
        CannonPower = MolecularSimulationConstants.DefaultCannonPower;
        IsPaused = false;
        IsCannonFiring = false;
    }

    private string GetInitialStateButtonClass(MolecularInitialState state) =>
        state == SelectedInitialState ? "md-state-button md-state-button--active" : "md-state-button";

    private static string GetInitialStateTitle(MolecularInitialState state) => state switch
    {
        MolecularInitialState.FreeGas => "Free gas",
        MolecularInitialState.BondedLayer => "Bonded layer",
        MolecularInitialState.SolidBlock => "Solid block",
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
    };

    private static string GetInitialStateDescription(MolecularInitialState state) => state switch
    {
        MolecularInitialState.FreeGas => "Random distribution with no initial bonds.",
        MolecularInitialState.BondedLayer => "A cold bonded layer at the bottom and free particles above it.",
        MolecularInitialState.SolidBlock => "Dense crystal-like packing that resists deformation at first.",
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
    };
}
