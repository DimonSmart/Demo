﻿using Demo.Demos.TSM.GeneralGenetic;

namespace Demo.Demos.TSM
{
    public class Genetic<T> where T : class, IChromosome<T>
    {
        private readonly ChromosomeWithScore<T>[] _chromosomes;
        private readonly GeneticAlgorithmSettings _settings;
        private readonly ChromosomeRanges _ranges;

        private readonly IChromosomeMutator<T> _mutator;
        private readonly IChromosomeCrossoverer<T> _crossoverer;
        private readonly IChromosomeFactory<T> _factory;
        private readonly IFitnessFunction<T> _fitnessFunction;

        public Genetic(GeneticAlgorithmSettings geneticAlgorithmSettings,
            IChromosomeMutator<T> mutator,
            IChromosomeCrossoverer<T> crossoverer,
            IChromosomeFactory<T> factory,
            IFitnessFunction<T> fitnessFunction)
        {
            _settings = geneticAlgorithmSettings;
            _mutator = mutator;
            _crossoverer = crossoverer;
            _factory = factory;
            _fitnessFunction = fitnessFunction;
            _ranges = new ChromosomeRanges(_settings);
            _chromosomes = new ChromosomeWithScore<T>[_settings.Population];
            SetNewChromosomes(0, _settings.Population);
        }

        public int InitialScore => int.MaxValue;

        public ChromosomeWithScore<T> GetBestResult() => _chromosomes.First();

        public IReadOnlyCollection<ChromosomeWithScore<T>> GetResults() => _chromosomes.ToList();

        public void NextIteration()
        {
            Mutate();
            AddNewChromosomes();
            Crossover();
            UpdateScore();
            Sort();
        }

        private void Crossover()
        {
            var from = _ranges.CrossoverChromosomesStartIndex;
            var to = _ranges.CrossoverChromosomesStartIndex + _ranges.CrossoverChromosomesCount;
            for (var i = from; i < to; i++)
            {
                var a = _chromosomes[Random.Shared.Next(_ranges.BestChromosomesStartIndex, _ranges.MutationChromosomesStartIndex)];
                var b = _chromosomes[Random.Shared.Next(_ranges.BestChromosomesStartIndex, _ranges.MutationChromosomesStartIndex)];
                if (a == b)
                {
                    continue;
                }

                _crossoverer.ApplyCrossover(_chromosomes[i].Chromosome, b.Chromosome);
            }
        }

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            foreach (var chromosomeWithScore in _chromosomes)
            {
                sb.AppendFormat("G:{0} S:{1}", chromosomeWithScore.Chromosome, chromosomeWithScore.Score);
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private void AddNewChromosomes() =>
            SetNewChromosomes(_ranges.NewChromosomesStartIndex, _ranges.NewChromosomesStartIndex + _ranges.NewChromosomesCount);

        private void Mutate()
        {
            if (_settings.MutationChromosomes == 0) return;
            for (var i = _ranges.MutationChromosomesStartIndex; i < _ranges.MutationChromosomesStartIndex + _ranges.MutationChromosomesCount; i++)
            {
                _mutator.Mutate(_chromosomes[i].Chromosome);
                _chromosomes[i].Score = InitialScore;
            }
        }

        private void SetNewChromosomes(int from, int to)
        {
            for (var i = from; i < to; i++)
            {
                _chromosomes[i] = new ChromosomeWithScore<T>
                {
                    Chromosome = _factory.Create(),
                    Score = InitialScore
                };
            }
        }

        private void Sort() => Array.Sort(_chromosomes, (a, b) => a.Score - b.Score);

        private void UpdateScore()
        {
            foreach (var chromosomeWithScore in _chromosomes.Where(i => i.Score == InitialScore))
            {
                chromosomeWithScore.Score = _fitnessFunction.CalculateFitness(chromosomeWithScore.Chromosome);
            }
        }
    }
}