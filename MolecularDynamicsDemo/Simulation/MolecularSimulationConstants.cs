namespace MolecularDynamicsDemo.Simulation;

internal static class MolecularSimulationConstants
{
    public const float DefaultViewportWidth = 960f;
    public const float DefaultViewportHeight = 640f;
    public const float MinimumViewportWidth = 41f;
    public const float MinimumViewportHeight = 71f;

    public const int DefaultParticleCount = 800;
    public const int MinimumParticleCount = 50;
    public const int MaximumParticleCount = 3000;

    public const float ParticleRadius = 2f;
    public const float BondDistance = 12f;
    public const float BondDistanceSquared = BondDistance * BondDistance;
    public const float RepulsionDistance = 9f;
    public const float RepulsionDistanceSquared = RepulsionDistance * RepulsionDistance;
    public const float MinimumDistance = 6f;
    public const float BreakDistance = 18f;
    public const float SpringConstant = 0.06f;
    public const float SpatialCellSize = 25f;
    public const float WallThickness = 10f;
    public const float InitialPadding = 20f;
    public const float InitialBottomMargin = 50f;
    public const float LayoutSpacing = 10f;

    public const int MaxBondsPerParticle = 6;
    public const float BondCreationRelativeEnergyThreshold = 1.5f;

    public const float DefaultWallTemperature = 1f;
    public const float MinimumWallTemperature = 0.1f;
    public const float MaximumWallTemperature = 20f;

    public const float DefaultCannonAngleDegrees = 90f;
    public const float MinimumCannonAngleDegrees = 0f;
    public const float MaximumCannonAngleDegrees = 180f;
    public const float DefaultCannonPower = 5f;
    public const float MinimumCannonPower = 1f;
    public const float MaximumCannonPower = 15f;
    public const float CannonEmissionInterval = 6f;
    public const float MaximumNormalizedTimeStep = 1.5f;
    public const float MinimumNormalizedTimeStep = 0.25f;

    public const int ExtraParticleCapacity = 512;
    public const int ExtraBondCapacity = 1024;
}
