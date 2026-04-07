using System.Runtime.CompilerServices;

namespace MolecularDynamicsDemo.Simulation;

public sealed class MolecularSimulation(Random? random = null)
{
    private readonly Random random = random ?? new Random();
    private ParticleState[] particles = [];
    private BondState[] bonds = [];
    private readonly Dictionary<long, int> bondLookup = new();
    private int[] bondCounts = [];
    private int[] nextParticleInCell = [];
    private int[] gridCellHeads = [];

    private int particleCount;
    private int bondCount;
    private int gridColumns;
    private int gridRows;
    private float fireAccumulator;

    public float Width { get; private set; } = MolecularSimulationConstants.DefaultViewportWidth;

    public float Height { get; private set; } = MolecularSimulationConstants.DefaultViewportHeight;

    public float LeftWallTemperature { get; set; } = MolecularSimulationConstants.DefaultWallTemperature;

    public float RightWallTemperature { get; set; } = MolecularSimulationConstants.DefaultWallTemperature;

    public float CannonAngleDegrees { get; set; } = MolecularSimulationConstants.DefaultCannonAngleDegrees;

    public float CannonPower { get; set; } = MolecularSimulationConstants.DefaultCannonPower;

    public bool IsPaused { get; set; }

    public bool IsCannonFiring { get; set; }

    public MolecularSimulationStats Statistics { get; private set; } = new(0, 0, 0f, 0f);

    internal ReadOnlySpan<ParticleState> GetParticles() => particles.AsSpan(0, particleCount);

    internal ReadOnlySpan<BondState> GetBonds() => bonds.AsSpan(0, bondCount);

    public void Initialize(MolecularSimulationSession session, float width, float height)
    {
        ArgumentNullException.ThrowIfNull(session);
        ValidateViewportDimensions(width, height);

        if (session.ParticleCount is < MolecularSimulationConstants.MinimumParticleCount or > MolecularSimulationConstants.MaximumParticleCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(session.ParticleCount),
                session.ParticleCount,
                $"Particle count must be between {MolecularSimulationConstants.MinimumParticleCount} and {MolecularSimulationConstants.MaximumParticleCount}.");
        }

        Width = width;
        Height = height;
        LeftWallTemperature = ClampTemperature(LeftWallTemperature);
        RightWallTemperature = ClampTemperature(RightWallTemperature);
        CannonAngleDegrees = ClampCannonAngle(CannonAngleDegrees);
        CannonPower = ClampCannonPower(CannonPower);
        IsPaused = false;
        IsCannonFiring = false;
        fireAccumulator = 0f;

        particleCount = 0;
        bondCount = 0;
        bondLookup.Clear();

        EnsureParticleCapacity(session.ParticleCount + MolecularSimulationConstants.ExtraParticleCapacity);
        EnsureBondCapacity(session.ParticleCount * MolecularSimulationConstants.MaxBondsPerParticle + MolecularSimulationConstants.ExtraBondCapacity);
        Array.Clear(bondCounts, 0, bondCounts.Length);

        switch (session.InitialState)
        {
            case MolecularInitialState.FreeGas:
                InitializeFreeGas(session.ParticleCount);
                break;
            case MolecularInitialState.BondedLayer:
                InitializeBondedLayer(session.ParticleCount);
                break;
            case MolecularInitialState.SolidBlock:
                InitializeSolidBlock(session.ParticleCount);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(session.InitialState), session.InitialState, "Unknown molecular initial state.");
        }

        if (session.InitialState != MolecularInitialState.FreeGas)
        {
            CreateInitialBonds();
        }

        Statistics = new(particleCount, bondCount, 0f, 0f);
    }

    public void Resize(float width, float height)
    {
        ValidateViewportDimensions(width, height);

        Width = width;
        Height = height;

        for (var particleIndex = 0; particleIndex < particleCount; particleIndex++)
        {
            ClampParticleInsideChamber(ref particles[particleIndex]);
        }
    }

    public int? FindNearestParticleIndex(float x, float y, float captureRadius)
    {
        ValidateFiniteValue(x, nameof(x));
        ValidateFiniteValue(y, nameof(y));

        if (!float.IsFinite(captureRadius) || captureRadius <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(captureRadius), captureRadius, "Capture radius must be finite and positive.");
        }

        var maximumDistanceSquared = captureRadius * captureRadius;
        int? nearestParticleIndex = null;
        var nearestDistanceSquared = maximumDistanceSquared;

        for (var particleIndex = 0; particleIndex < particleCount; particleIndex++)
        {
            var dx = particles[particleIndex].X - x;
            var dy = particles[particleIndex].Y - y;
            var distanceSquared = dx * dx + dy * dy;

            if (distanceSquared < nearestDistanceSquared)
            {
                nearestDistanceSquared = distanceSquared;
                nearestParticleIndex = particleIndex;
            }
        }

        return nearestParticleIndex;
    }

    public void MoveParticle(int particleIndex, float x, float y, float velocityX, float velocityY)
    {
        if (particleIndex < 0 || particleIndex >= particleCount)
        {
            throw new ArgumentOutOfRangeException(nameof(particleIndex), particleIndex, "Particle index is outside the active particle range.");
        }

        ValidateFiniteValue(x, nameof(x));
        ValidateFiniteValue(y, nameof(y));
        ValidateFiniteValue(velocityX, nameof(velocityX));
        ValidateFiniteValue(velocityY, nameof(velocityY));

        ref var particle = ref particles[particleIndex];
        particle.X = x;
        particle.Y = y;
        particle.VelocityX = velocityX;
        particle.VelocityY = velocityY;
        ClampParticleInsideChamber(ref particle);
    }

    public int Step(float normalizedTimeStep)
    {
        var dt = Math.Clamp(
            normalizedTimeStep,
            MolecularSimulationConstants.MinimumNormalizedTimeStep,
            MolecularSimulationConstants.MaximumNormalizedTimeStep);

        LeftWallTemperature = ClampTemperature(LeftWallTemperature);
        RightWallTemperature = ClampTemperature(RightWallTemperature);
        CannonAngleDegrees = ClampCannonAngle(CannonAngleDegrees);
        CannonPower = ClampCannonPower(CannonPower);

        if (particleCount == 0)
        {
            Statistics = new(0, bondCount, 0f, 0f);
            return 0;
        }

        if (IsPaused)
        {
            Statistics = new(particleCount, bondCount, Statistics.AverageSpeedSquared, 0f);
            return 0;
        }

        var emittedParticleCount = EmitCannonParticles(dt);
        ApplyBondForces(dt);
        var totalKineticEnergy = UpdatePositionsAndWalls(dt);
        ApplyRepulsionAndBondFormation(dt);
        Statistics = new(particleCount, bondCount, totalKineticEnergy / Math.Max(1, particleCount), 0f);
        return emittedParticleCount;
    }

    private void InitializeFreeGas(int requestedParticleCount)
    {
        for (var particleIndex = 0; particleIndex < requestedParticleCount; particleIndex++)
        {
            particles[particleIndex] = new ParticleState
            {
                X = RandomInRange(MolecularSimulationConstants.InitialPadding, Width - MolecularSimulationConstants.InitialPadding),
                Y = RandomInRange(MolecularSimulationConstants.InitialPadding, Height - MolecularSimulationConstants.InitialPadding),
                VelocityX = RandomCentered() * 3f,
                VelocityY = RandomCentered() * 3f
            };
        }

        particleCount = requestedParticleCount;
    }

    private void InitializeBondedLayer(int requestedParticleCount)
    {
        var bondedTarget = Math.Max(1, requestedParticleCount / 2);
        var bottomRows = Math.Max(1, (int)MathF.Sqrt(bondedTarget));
        var maxParticlesPerRow = Math.Max(1, (int)((Width - 2f * (MolecularSimulationConstants.WallThickness + MolecularSimulationConstants.InitialPadding)) / MolecularSimulationConstants.LayoutSpacing));
        var particlesPerRow = Math.Clamp(Math.Max(1, bondedTarget / bottomRows), 1, maxParticlesPerRow);
        var bondedCount = Math.Min(requestedParticleCount, bottomRows * particlesPerRow);
        var bondedLayerWidth = particlesPerRow * MolecularSimulationConstants.LayoutSpacing;
        var xStart = (Width - bondedLayerWidth) * 0.5f;

        for (var particleIndex = 0; particleIndex < bondedCount; particleIndex++)
        {
            var row = particleIndex / particlesPerRow;
            var column = particleIndex % particlesPerRow;

            particles[particleIndex] = new ParticleState
            {
                X = xStart + column * MolecularSimulationConstants.LayoutSpacing,
                Y = Height - MolecularSimulationConstants.InitialBottomMargin - row * MolecularSimulationConstants.LayoutSpacing,
                VelocityX = RandomCentered() * 0.5f,
                VelocityY = RandomCentered() * 0.5f
            };

            ClampParticleInsideChamber(ref particles[particleIndex]);
        }

        for (var particleIndex = bondedCount; particleIndex < requestedParticleCount; particleIndex++)
        {
            particles[particleIndex] = new ParticleState
            {
                X = RandomInRange(MolecularSimulationConstants.InitialPadding, Width - MolecularSimulationConstants.InitialPadding),
                Y = RandomInRange(MolecularSimulationConstants.InitialPadding, Height * 0.5f),
                VelocityX = RandomCentered() * 3f,
                VelocityY = RandomCentered() * 3f
            };
        }

        particleCount = requestedParticleCount;
    }

    private void InitializeSolidBlock(int requestedParticleCount)
    {
        var aspectRatio = Width / Height;
        var maxParticlesPerRow = Math.Max(1, (int)((Width - 2f * (MolecularSimulationConstants.WallThickness + MolecularSimulationConstants.InitialPadding)) / MolecularSimulationConstants.LayoutSpacing));
        var particlesPerRow = Math.Clamp((int)MathF.Sqrt(requestedParticleCount * aspectRatio), 1, maxParticlesPerRow);
        var layerWidth = particlesPerRow * MolecularSimulationConstants.LayoutSpacing;
        var xStart = (Width - layerWidth) * 0.5f;

        for (var particleIndex = 0; particleIndex < requestedParticleCount; particleIndex++)
        {
            var row = particleIndex / particlesPerRow;
            var column = particleIndex % particlesPerRow;

            particles[particleIndex] = new ParticleState
            {
                X = xStart + column * MolecularSimulationConstants.LayoutSpacing,
                Y = Height - MolecularSimulationConstants.InitialBottomMargin - row * MolecularSimulationConstants.LayoutSpacing,
                VelocityX = RandomCentered() * 0.3f,
                VelocityY = RandomCentered() * 0.3f
            };

            ClampParticleInsideChamber(ref particles[particleIndex]);
        }

        particleCount = requestedParticleCount;
    }

    private void CreateInitialBonds()
    {
        BuildSpatialGrid();

        for (var particleIndex = 0; particleIndex < particleCount; particleIndex++)
        {
            ref var particle = ref particles[particleIndex];
            var cellX = ClampToGridColumn((int)(particle.X / MolecularSimulationConstants.SpatialCellSize));
            var cellY = ClampToGridRow((int)(particle.Y / MolecularSimulationConstants.SpatialCellSize));

            for (var offsetY = -1; offsetY <= 1; offsetY++)
            {
                for (var offsetX = -1; offsetX <= 1; offsetX++)
                {
                    var neighbourX = cellX + offsetX;
                    var neighbourY = cellY + offsetY;

                    if (neighbourX < 0 || neighbourX >= gridColumns || neighbourY < 0 || neighbourY >= gridRows)
                    {
                        continue;
                    }

                    for (var neighbourParticleIndex = gridCellHeads[neighbourY * gridColumns + neighbourX];
                         neighbourParticleIndex >= 0;
                         neighbourParticleIndex = nextParticleInCell[neighbourParticleIndex])
                    {
                        if (particleIndex >= neighbourParticleIndex)
                        {
                            continue;
                        }

                        ref var neighbour = ref particles[neighbourParticleIndex];
                        var dx = neighbour.X - particle.X;
                        var dy = neighbour.Y - particle.Y;
                        var distanceSquared = dx * dx + dy * dy;

                        if (distanceSquared < MolecularSimulationConstants.BondDistanceSquared)
                        {
                            AddBond(particleIndex, neighbourParticleIndex);
                        }
                    }
                }
            }
        }
    }

    private int EmitCannonParticles(float dt)
    {
        if (!IsCannonFiring)
        {
            fireAccumulator = 0f;
            return 0;
        }

        fireAccumulator += dt;
        var emittedParticleCount = 0;

        while (fireAccumulator >= MolecularSimulationConstants.CannonEmissionInterval)
        {
            fireAccumulator -= MolecularSimulationConstants.CannonEmissionInterval;
            SpawnCannonParticle();
            emittedParticleCount++;
        }

        return emittedParticleCount;
    }

    private void SpawnCannonParticle()
    {
        EnsureParticleCapacity(particleCount + 1);
        var angleRadians = DegreesToRadians(180f - CannonAngleDegrees);

        particles[particleCount] = new ParticleState
        {
            X = Width * 0.5f,
            Y = Height - MolecularSimulationConstants.WallThickness,
            VelocityX = MathF.Cos(angleRadians) * CannonPower,
            VelocityY = -MathF.Sin(angleRadians) * CannonPower
        };

        bondCounts[particleCount] = 0;
        particleCount++;
    }

    private void ApplyBondForces(float dt)
    {
        var thermalBreakThreshold = 2.5f + (LeftWallTemperature + RightWallTemperature) * 0.2f;

        for (var bondIndex = 0; bondIndex < bondCount;)
        {
            ref var bond = ref bonds[bondIndex];
            ref var firstParticle = ref particles[bond.FirstParticleIndex];
            ref var secondParticle = ref particles[bond.SecondParticleIndex];

            var dx = secondParticle.X - firstParticle.X;
            var dy = secondParticle.Y - firstParticle.Y;
            var distance = MathF.Sqrt(dx * dx + dy * dy);
            var relativeVelocityX = firstParticle.VelocityX - secondParticle.VelocityX;
            var relativeVelocityY = firstParticle.VelocityY - secondParticle.VelocityY;
            var relativeEnergy = relativeVelocityX * relativeVelocityX + relativeVelocityY * relativeVelocityY;

            if (distance > MolecularSimulationConstants.BreakDistance ||
                distance < 0.1f ||
                relativeEnergy > thermalBreakThreshold)
            {
                RemoveBondAt(bondIndex);
                continue;
            }

            var inverseDistance = 1f / distance;
            var force = (distance - MolecularSimulationConstants.BondDistance) * MolecularSimulationConstants.SpringConstant * dt;
            var forceX = dx * inverseDistance * force;
            var forceY = dy * inverseDistance * force;

            firstParticle.VelocityX += forceX;
            firstParticle.VelocityY += forceY;
            secondParticle.VelocityX -= forceX;
            secondParticle.VelocityY -= forceY;
            bondIndex++;
        }
    }

    private float UpdatePositionsAndWalls(float dt)
    {
        var totalKineticEnergy = 0f;

        for (var particleIndex = 0; particleIndex < particleCount; particleIndex++)
        {
            ref var particle = ref particles[particleIndex];
            particle.X += particle.VelocityX * dt;
            particle.Y += particle.VelocityY * dt;

            if (particle.X < MolecularSimulationConstants.WallThickness)
            {
                particle.X = MolecularSimulationConstants.WallThickness;
                ApplyThermalReflection(ref particle, LeftWallTemperature, true);
            }
            else if (particle.X > Width - MolecularSimulationConstants.WallThickness)
            {
                particle.X = Width - MolecularSimulationConstants.WallThickness;
                ApplyThermalReflection(ref particle, RightWallTemperature, false);
            }

            if (particle.Y < MolecularSimulationConstants.WallThickness)
            {
                particle.Y = MolecularSimulationConstants.WallThickness;
                particle.VelocityY = MathF.Abs(particle.VelocityY);
            }
            else if (particle.Y > Height - MolecularSimulationConstants.WallThickness)
            {
                particle.Y = Height - MolecularSimulationConstants.WallThickness;
                particle.VelocityY = -MathF.Abs(particle.VelocityY);
            }

            totalKineticEnergy += particle.VelocityX * particle.VelocityX + particle.VelocityY * particle.VelocityY;
        }

        return totalKineticEnergy;
    }

    private void ApplyRepulsionAndBondFormation(float dt)
    {
        BuildSpatialGrid();

        for (var particleIndex = 0; particleIndex < particleCount; particleIndex++)
        {
            ref var particle = ref particles[particleIndex];
            var cellX = ClampToGridColumn((int)(particle.X / MolecularSimulationConstants.SpatialCellSize));
            var cellY = ClampToGridRow((int)(particle.Y / MolecularSimulationConstants.SpatialCellSize));

            for (var offsetY = -1; offsetY <= 1; offsetY++)
            {
                for (var offsetX = -1; offsetX <= 1; offsetX++)
                {
                    var neighbourX = cellX + offsetX;
                    var neighbourY = cellY + offsetY;

                    if (neighbourX < 0 || neighbourX >= gridColumns || neighbourY < 0 || neighbourY >= gridRows)
                    {
                        continue;
                    }

                    for (var neighbourParticleIndex = gridCellHeads[neighbourY * gridColumns + neighbourX];
                         neighbourParticleIndex >= 0;
                         neighbourParticleIndex = nextParticleInCell[neighbourParticleIndex])
                    {
                        if (particleIndex >= neighbourParticleIndex)
                        {
                            continue;
                        }

                        ref var neighbour = ref particles[neighbourParticleIndex];
                        var dx = neighbour.X - particle.X;
                        var dy = neighbour.Y - particle.Y;
                        var distanceSquared = dx * dx + dy * dy;

                        if (distanceSquared < MolecularSimulationConstants.RepulsionDistanceSquared)
                        {
                            ApplyRepulsion(ref particle, ref neighbour, dx, dy, distanceSquared, dt);
                            continue;
                        }

                        if (distanceSquared < MolecularSimulationConstants.BondDistanceSquared)
                        {
                            TryCreateBond(particleIndex, neighbourParticleIndex);
                        }
                    }
                }
            }
        }
    }

    private void ApplyRepulsion(ref ParticleState particle, ref ParticleState neighbour, float dx, float dy, float distanceSquared, float dt)
    {
        var distance = MathF.Sqrt(MathF.Max(distanceSquared, 0.01f));
        var normalX = dx / distance;
        var normalY = dy / distance;

        if (distance < MolecularSimulationConstants.MinimumDistance)
        {
            var push = (MolecularSimulationConstants.MinimumDistance - distance) * 0.5f;
            particle.X -= normalX * push;
            particle.Y -= normalY * push;
            neighbour.X += normalX * push;
            neighbour.Y += normalY * push;
            ClampParticleInsideChamber(ref particle);
            ClampParticleInsideChamber(ref neighbour);
        }

        var overlap = MolecularSimulationConstants.RepulsionDistance - distance;
        var forceX = normalX * overlap * 0.2f * dt;
        var forceY = normalY * overlap * 0.2f * dt;

        particle.VelocityX -= forceX;
        particle.VelocityY -= forceY;
        neighbour.VelocityX += forceX;
        neighbour.VelocityY += forceY;
    }

    private void TryCreateBond(int firstParticleIndex, int secondParticleIndex)
    {
        if (bondCounts[firstParticleIndex] >= MolecularSimulationConstants.MaxBondsPerParticle ||
            bondCounts[secondParticleIndex] >= MolecularSimulationConstants.MaxBondsPerParticle)
        {
            return;
        }

        var bondKey = MakeBondKey(firstParticleIndex, secondParticleIndex);
        if (bondLookup.ContainsKey(bondKey))
        {
            return;
        }

        ref var firstParticle = ref particles[firstParticleIndex];
        ref var secondParticle = ref particles[secondParticleIndex];
        var relativeVelocityX = firstParticle.VelocityX - secondParticle.VelocityX;
        var relativeVelocityY = firstParticle.VelocityY - secondParticle.VelocityY;
        var relativeEnergy = relativeVelocityX * relativeVelocityX + relativeVelocityY * relativeVelocityY;

        if (relativeEnergy < MolecularSimulationConstants.BondCreationRelativeEnergyThreshold)
        {
            AddBond(firstParticleIndex, secondParticleIndex);
        }
    }

    private void BuildSpatialGrid()
    {
        gridColumns = Math.Max(1, (int)MathF.Ceiling(Width / MolecularSimulationConstants.SpatialCellSize));
        gridRows = Math.Max(1, (int)MathF.Ceiling(Height / MolecularSimulationConstants.SpatialCellSize));
        var totalCellCount = gridColumns * gridRows;

        if (gridCellHeads.Length < totalCellCount)
        {
            Array.Resize(ref gridCellHeads, totalCellCount);
        }

        Array.Fill(gridCellHeads, -1, 0, totalCellCount);

        for (var particleIndex = 0; particleIndex < particleCount; particleIndex++)
        {
            var cellX = ClampToGridColumn((int)(particles[particleIndex].X / MolecularSimulationConstants.SpatialCellSize));
            var cellY = ClampToGridRow((int)(particles[particleIndex].Y / MolecularSimulationConstants.SpatialCellSize));
            var cellIndex = cellY * gridColumns + cellX;
            nextParticleInCell[particleIndex] = gridCellHeads[cellIndex];
            gridCellHeads[cellIndex] = particleIndex;
        }
    }

    private void AddBond(int firstParticleIndex, int secondParticleIndex)
    {
        EnsureBondCapacity(bondCount + 1);
        var bondKey = MakeBondKey(firstParticleIndex, secondParticleIndex);

        bonds[bondCount] = new BondState
        {
            FirstParticleIndex = firstParticleIndex,
            SecondParticleIndex = secondParticleIndex
        };

        bondLookup[bondKey] = bondCount;
        bondCounts[firstParticleIndex]++;
        bondCounts[secondParticleIndex]++;
        bondCount++;
    }

    private void RemoveBondAt(int bondIndex)
    {
        var removedBond = bonds[bondIndex];
        bondLookup.Remove(MakeBondKey(removedBond.FirstParticleIndex, removedBond.SecondParticleIndex));
        bondCounts[removedBond.FirstParticleIndex]--;
        bondCounts[removedBond.SecondParticleIndex]--;

        var lastBondIndex = bondCount - 1;
        if (bondIndex != lastBondIndex)
        {
            var lastBond = bonds[lastBondIndex];
            bonds[bondIndex] = lastBond;
            bondLookup[MakeBondKey(lastBond.FirstParticleIndex, lastBond.SecondParticleIndex)] = bondIndex;
        }

        bondCount--;
    }

    private void EnsureParticleCapacity(int requiredCapacity)
    {
        if (particles.Length >= requiredCapacity)
        {
            return;
        }

        var newCapacity = Math.Max(requiredCapacity, particles.Length == 0 ? 1024 : particles.Length * 2);
        Array.Resize(ref particles, newCapacity);
        Array.Resize(ref bondCounts, newCapacity);
        Array.Resize(ref nextParticleInCell, newCapacity);
    }

    private void EnsureBondCapacity(int requiredCapacity)
    {
        if (bonds.Length >= requiredCapacity)
        {
            return;
        }

        var newCapacity = Math.Max(requiredCapacity, bonds.Length == 0 ? 2048 : bonds.Length * 2);
        Array.Resize(ref bonds, newCapacity);
    }

    private static long MakeBondKey(int firstParticleIndex, int secondParticleIndex)
    {
        var minIndex = Math.Min(firstParticleIndex, secondParticleIndex);
        var maxIndex = Math.Max(firstParticleIndex, secondParticleIndex);
        return ((long)minIndex << 32) | (uint)maxIndex;
    }

    private void ApplyThermalReflection(ref ParticleState particle, float wallTemperature, bool isLeftWall)
    {
        var speed = MathF.Sqrt(particle.VelocityX * particle.VelocityX + particle.VelocityY * particle.VelocityY);
        var targetSpeed = MathF.Sqrt(wallTemperature * 4f);
        var newSpeed = speed * 0.1f + targetSpeed * 0.9f;
        var ratio = newSpeed / (speed + 0.001f);

        particle.VelocityX = isLeftWall
            ? MathF.Abs(particle.VelocityX) * ratio
            : -MathF.Abs(particle.VelocityX) * ratio;
        particle.VelocityY *= ratio;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ClampParticleInsideChamber(ref ParticleState particle)
    {
        particle.X = Math.Clamp(particle.X, MolecularSimulationConstants.WallThickness, Width - MolecularSimulationConstants.WallThickness);
        particle.Y = Math.Clamp(particle.Y, MolecularSimulationConstants.WallThickness, Height - MolecularSimulationConstants.WallThickness);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int ClampToGridColumn(int cellX) => Math.Clamp(cellX, 0, gridColumns - 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int ClampToGridRow(int cellY) => Math.Clamp(cellY, 0, gridRows - 1);

    private float RandomCentered() => (float)(random.NextDouble() - 0.5d);

    private float RandomInRange(float minValue, float maxValue) => minValue + (float)random.NextDouble() * (maxValue - minValue);

    private static void ValidateViewportDimensions(float width, float height)
    {
        if (!float.IsFinite(width) || width < MolecularSimulationConstants.MinimumViewportWidth)
        {
            throw new ArgumentOutOfRangeException(
                nameof(width),
                width,
                $"Viewport width must be finite and at least {MolecularSimulationConstants.MinimumViewportWidth}.");
        }

        if (!float.IsFinite(height) || height < MolecularSimulationConstants.MinimumViewportHeight)
        {
            throw new ArgumentOutOfRangeException(
                nameof(height),
                height,
                $"Viewport height must be finite and at least {MolecularSimulationConstants.MinimumViewportHeight}.");
        }
    }

    private static void ValidateFiniteValue(float value, string parameterName)
    {
        if (!float.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Value must be finite.");
        }
    }

    private static float DegreesToRadians(float degrees) => degrees * (MathF.PI / 180f);

    private static float ClampTemperature(float temperature) =>
        Math.Clamp(temperature, MolecularSimulationConstants.MinimumWallTemperature, MolecularSimulationConstants.MaximumWallTemperature);

    private static float ClampCannonAngle(float angleDegrees) =>
        Math.Clamp(angleDegrees, MolecularSimulationConstants.MinimumCannonAngleDegrees, MolecularSimulationConstants.MaximumCannonAngleDegrees);

    private static float ClampCannonPower(float power) =>
        Math.Clamp(power, MolecularSimulationConstants.MinimumCannonPower, MolecularSimulationConstants.MaximumCannonPower);
}
