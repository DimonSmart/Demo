using SkiaSharp;

namespace MolecularDynamicsDemo.Simulation;

public sealed class MolecularSimulationRenderer : IDisposable
{
    private readonly SKPaint backgroundPaint = new();
    private readonly SKPaint chamberBorderPaint = new()
    {
        Color = new SKColor(148, 163, 184, 210),
        IsAntialias = true,
        StrokeWidth = 2f,
        Style = SKPaintStyle.Stroke
    };
    private readonly SKPaint wallPaint = new()
    {
        IsAntialias = true,
        Style = SKPaintStyle.Fill
    };
    private readonly SKPaint bondPaint = new()
    {
        Color = new SKColor(125, 140, 160, 180),
        IsAntialias = false,
        StrokeWidth = 1f,
        Style = SKPaintStyle.Stroke
    };
    private readonly SKPaint particlePaint = new()
    {
        IsAntialias = false,
        Style = SKPaintStyle.Fill
    };
    private readonly SKPaint cannonBodyPaint = new()
    {
        IsAntialias = true,
        Style = SKPaintStyle.Fill
    };
    private readonly SKPaint cannonOutlinePaint = new()
    {
        Color = SKColors.White.WithAlpha(180),
        IsAntialias = true,
        StrokeWidth = 2f,
        Style = SKPaintStyle.Stroke
    };
    private readonly SKPaint muzzleFlashPaint = new()
    {
        IsAntialias = true,
        Style = SKPaintStyle.Fill
    };
    private readonly SKPaint overlayPaint = new()
    {
        IsAntialias = true,
        Color = new SKColor(255, 255, 255, 20),
        StrokeWidth = 1f,
        Style = SKPaintStyle.Stroke
    };

    public void Render(SKCanvas canvas, SKImageInfo info, MolecularSimulation simulation, float elapsedSeconds, float cannonFlashIntensity)
    {
        backgroundPaint.Shader?.Dispose();
        backgroundPaint.Shader = SKShader.CreateLinearGradient(
            new SKPoint(0f, 0f),
            new SKPoint(0f, info.Height),
            [new SKColor(8, 16, 33), new SKColor(9, 23, 44), new SKColor(3, 8, 19)],
            [0f, 0.55f, 1f],
            SKShaderTileMode.Clamp);

        canvas.Clear();
        canvas.DrawRect(SKRect.Create(info.Width, info.Height), backgroundPaint);

        DrawChamber(canvas, info, simulation);
        DrawBonds(canvas, simulation);
        DrawParticles(canvas, simulation);
        DrawCannon(canvas, simulation, elapsedSeconds, cannonFlashIntensity);
    }

    private void DrawChamber(SKCanvas canvas, SKImageInfo info, MolecularSimulation simulation)
    {
        var chamber = SKRect.Create(0f, 0f, info.Width, info.Height);
        canvas.DrawRoundRect(new SKRoundRect(chamber, 18f, 18f), chamberBorderPaint);

        wallPaint.Color = CreateWallColor(simulation.LeftWallTemperature);
        canvas.DrawRect(SKRect.Create(0f, 0f, MolecularSimulationConstants.WallThickness, info.Height), wallPaint);

        wallPaint.Color = CreateWallColor(simulation.RightWallTemperature);
        canvas.DrawRect(
            SKRect.Create(info.Width - MolecularSimulationConstants.WallThickness, 0f, MolecularSimulationConstants.WallThickness, info.Height),
            wallPaint);

        const float guideStep = 36f;
        for (var x = guideStep; x < info.Width; x += guideStep)
        {
            canvas.DrawLine(x, 0f, x, info.Height, overlayPaint);
        }

        for (var y = guideStep; y < info.Height; y += guideStep)
        {
            canvas.DrawLine(0f, y, info.Width, y, overlayPaint);
        }
    }

    private void DrawBonds(SKCanvas canvas, MolecularSimulation simulation)
    {
        var particles = simulation.GetParticles();

        foreach (ref readonly var bond in simulation.GetBonds())
        {
            ref readonly var firstParticle = ref particles[bond.FirstParticleIndex];
            ref readonly var secondParticle = ref particles[bond.SecondParticleIndex];
            canvas.DrawLine(firstParticle.X, firstParticle.Y, secondParticle.X, secondParticle.Y, bondPaint);
        }
    }

    private void DrawParticles(SKCanvas canvas, MolecularSimulation simulation)
    {
        foreach (ref readonly var particle in simulation.GetParticles())
        {
            var speed = MathF.Sqrt(particle.VelocityX * particle.VelocityX + particle.VelocityY * particle.VelocityY);
            particlePaint.Color = CreateParticleColor(speed);
            canvas.DrawCircle(particle.X, particle.Y, MolecularSimulationConstants.ParticleRadius, particlePaint);
        }
    }

    private void DrawCannon(SKCanvas canvas, MolecularSimulation simulation, float elapsedSeconds, float cannonFlashIntensity)
    {
        var cannonBase = new SKPoint(simulation.Width * 0.5f, simulation.Height - MolecularSimulationConstants.WallThickness);
        var firing = simulation.IsCannonFiring;
        var cannonAngleRadians = (180f - simulation.CannonAngleDegrees) * (MathF.PI / 180f);

        canvas.Save();
        canvas.Translate(cannonBase.X, cannonBase.Y);
        canvas.RotateRadians(-cannonAngleRadians);

        cannonBodyPaint.Color = firing ? new SKColor(153, 27, 27) : new SKColor(71, 85, 105);
        canvas.DrawRect(SKRect.Create(0f, -6f, 35f, 12f), cannonBodyPaint);
        canvas.DrawRect(SKRect.Create(0f, -6f, 35f, 12f), cannonOutlinePaint);

        cannonBodyPaint.Color = firing ? new SKColor(245, 158, 11) : new SKColor(30, 41, 59);
        canvas.DrawRect(SKRect.Create(30f, -7f, 8f, 14f), cannonBodyPaint);

        if (cannonFlashIntensity > 0f)
        {
            var pulse = 4f + MathF.Sin(elapsedSeconds * 12f) * 3f;
            muzzleFlashPaint.Color = new SKColor(251, 191, 36, (byte)(cannonFlashIntensity * 255f));
            canvas.DrawCircle(42f, 0f, pulse * cannonFlashIntensity, muzzleFlashPaint);
        }

        canvas.Restore();

        cannonBodyPaint.Color = firing ? new SKColor(239, 68, 68) : new SKColor(59, 130, 246);
        canvas.DrawCircle(cannonBase, 15f, cannonBodyPaint);
        canvas.DrawCircle(cannonBase, 15f, cannonOutlinePaint);

        cannonOutlinePaint.Color = new SKColor(255, 255, 255, 64);
        cannonOutlinePaint.StrokeWidth = 1f;
        canvas.DrawCircle(cannonBase, 8f, cannonOutlinePaint);
        cannonOutlinePaint.Color = SKColors.White.WithAlpha(180);
        cannonOutlinePaint.StrokeWidth = 2f;
    }

    private static SKColor CreateWallColor(float temperature)
    {
        var ratio = Math.Clamp(
            (temperature - MolecularSimulationConstants.MinimumWallTemperature) /
            (MolecularSimulationConstants.MaximumWallTemperature - MolecularSimulationConstants.MinimumWallTemperature),
            0f,
            1f);

        var red = (byte)(70 + ratio * 185f);
        var blue = (byte)(255 - ratio * 180f);
        return new SKColor(red, 100, blue, 110);
    }

    private static SKColor CreateParticleColor(float speed)
    {
        var scaledSpeed = Math.Clamp(speed * 40f, 0f, 205f);
        var red = (byte)(50f + scaledSpeed);
        var blue = (byte)(255f - scaledSpeed);
        return new SKColor(red, 110, blue);
    }

    public void Dispose()
    {
        backgroundPaint.Shader?.Dispose();
        backgroundPaint.Dispose();
        chamberBorderPaint.Dispose();
        wallPaint.Dispose();
        bondPaint.Dispose();
        particlePaint.Dispose();
        cannonBodyPaint.Dispose();
        cannonOutlinePaint.Dispose();
        muzzleFlashPaint.Dispose();
        overlayPaint.Dispose();
    }
}
