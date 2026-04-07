using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MolecularDynamicsDemo.Simulation;
using SkiaSharp;
using SkiaSharp.Views.Blazor;

namespace MolecularDynamicsDemo.Components;

public partial class MolecularDynamicsViewport : ComponentBase, IDisposable
{
    private const float DragCaptureRadius = 40f;
    private const float PointerVelocityDecayFactor = 0.8f;
    private const float FlashDecayPerFrame = 0.15f;

    private readonly MolecularSimulation simulation = new();
    private readonly MolecularSimulationRenderer renderer = new();
    private readonly Stopwatch stopwatch = Stopwatch.StartNew();

    private TimeSpan previousFrameTime = TimeSpan.Zero;
    private TimeSpan previousStatisticsPublishTime = TimeSpan.Zero;
    private int framesSinceLastStatisticsPublish;
    private int activeSessionId = -1;
    private int? draggedParticleIndex;
    private float cannonFlashIntensity;
    private SKPoint pointerPosition;
    private SKPoint pointerVelocity;

    [Parameter, EditorRequired]
    public MolecularSimulationSession Session { get; set; } = default!;

    [Parameter]
    public bool IsPaused { get; set; }

    [Parameter]
    public float LeftWallTemperature { get; set; }

    [Parameter]
    public float RightWallTemperature { get; set; }

    [Parameter]
    public float CannonAngleDegrees { get; set; }

    [Parameter]
    public float CannonPower { get; set; }

    [Parameter]
    public bool IsCannonFiring { get; set; }

    [Parameter]
    public EventCallback<MolecularSimulationStats> StatisticsChanged { get; set; }

    protected override void OnParametersSet()
    {
        if (activeSessionId != Session.SessionId)
        {
            activeSessionId = Session.SessionId;
            simulation.Initialize(Session, simulation.Width, simulation.Height);
            previousFrameTime = TimeSpan.Zero;
            previousStatisticsPublishTime = stopwatch.Elapsed;
            framesSinceLastStatisticsPublish = 0;
            draggedParticleIndex = null;
            cannonFlashIntensity = 0f;
            pointerPosition = default;
            pointerVelocity = default;
            PublishStatistics(0f);
        }

        simulation.IsPaused = IsPaused;
        simulation.LeftWallTemperature = LeftWallTemperature;
        simulation.RightWallTemperature = RightWallTemperature;
        simulation.CannonAngleDegrees = CannonAngleDegrees;
        simulation.CannonPower = CannonPower;
        simulation.IsCannonFiring = IsCannonFiring;
    }

    private void HandlePaintSurface(SKPaintGLSurfaceEventArgs args)
    {
        if (args.Info.Width < MolecularSimulationConstants.MinimumViewportWidth ||
            args.Info.Height < MolecularSimulationConstants.MinimumViewportHeight)
        {
            return;
        }

        simulation.Resize(args.Info.Width, args.Info.Height);

        var elapsed = stopwatch.Elapsed;
        var deltaSeconds = previousFrameTime == TimeSpan.Zero
            ? 1f / 60f
            : (float)Math.Min((elapsed - previousFrameTime).TotalSeconds, 1d / 20d);

        previousFrameTime = elapsed;
        var normalizedTimeStep = deltaSeconds * 60f;
        ApplyDragging();
        var emittedParticleCount = simulation.Step(normalizedTimeStep);

        if (!simulation.IsPaused)
        {
            if (emittedParticleCount > 0)
            {
                cannonFlashIntensity = 1f;
            }

            cannonFlashIntensity = MathF.Max(0f, cannonFlashIntensity - FlashDecayPerFrame * normalizedTimeStep);
        }

        renderer.Render(args.Surface.Canvas, args.Info, simulation, (float)elapsed.TotalSeconds, cannonFlashIntensity);

        framesSinceLastStatisticsPublish++;
        if (elapsed - previousStatisticsPublishTime >= TimeSpan.FromMilliseconds(250))
        {
            var secondsSinceLastPublish = Math.Max((elapsed - previousStatisticsPublishTime).TotalSeconds, 0.001d);
            var framesPerSecond = (float)(framesSinceLastStatisticsPublish / secondsSinceLastPublish);
            previousStatisticsPublishTime = elapsed;
            framesSinceLastStatisticsPublish = 0;
            PublishStatistics(framesPerSecond);
        }
    }

    private void HandlePointerDown(PointerEventArgs args)
    {
        pointerPosition = new SKPoint((float)args.OffsetX, (float)args.OffsetY);
        pointerVelocity = default;
        draggedParticleIndex = simulation.FindNearestParticleIndex(pointerPosition.X, pointerPosition.Y, DragCaptureRadius);
    }

    private void HandlePointerMove(PointerEventArgs args)
    {
        if (args.Buttons == 0 || draggedParticleIndex is null)
        {
            return;
        }

        var nextPointerPosition = new SKPoint((float)args.OffsetX, (float)args.OffsetY);
        pointerVelocity = nextPointerPosition - pointerPosition;
        pointerPosition = nextPointerPosition;
    }

    private void HandlePointerUp(PointerEventArgs _)
    {
        draggedParticleIndex = null;
        pointerVelocity = default;
    }

    private void ApplyDragging()
    {
        if (draggedParticleIndex is not int activeParticleIndex)
        {
            return;
        }

        simulation.MoveParticle(activeParticleIndex, pointerPosition.X, pointerPosition.Y, pointerVelocity.X, pointerVelocity.Y);
        pointerVelocity = new SKPoint(pointerVelocity.X * PointerVelocityDecayFactor, pointerVelocity.Y * PointerVelocityDecayFactor);
    }

    private void PublishStatistics(float framesPerSecond)
    {
        if (!StatisticsChanged.HasDelegate)
        {
            return;
        }

        var snapshot = simulation.Statistics with
        {
            FramesPerSecond = framesPerSecond
        };

        _ = InvokeAsync(() => StatisticsChanged.InvokeAsync(snapshot));
    }

    public void Dispose()
    {
        renderer.Dispose();
    }
}
