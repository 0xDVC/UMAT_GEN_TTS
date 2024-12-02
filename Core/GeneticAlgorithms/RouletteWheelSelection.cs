using UMAT_GEN_TTS.Core.Interfaces;

namespace UMAT_GEN_TTS.Core.GeneticAlgorithms;

public class RouletteWheelSelection : ISelectionStrategy
{
    private readonly Random _random = new();

    public List<Chromosome> Select(List<Chromosome> population, int numberToSelect)
    {
        if (population == null || !population.Any())
            throw new ArgumentException("Population cannot be null or empty");

        if (numberToSelect <= 0 || numberToSelect > population.Count)
            throw new ArgumentException($"Invalid selection count: {numberToSelect}");

        var selected = new List<Chromosome>();
        var totalFitness = population.Sum(c => c.Fitness);

        if (totalFitness <= 0)
            throw new InvalidOperationException("Total fitness must be greater than 0");

        while (selected.Count < numberToSelect)
        {
            var randomValue = _random.NextDouble() * totalFitness;
            var runningSum = 0.0;

            foreach (var chromosome in population)
            {
                runningSum += chromosome.Fitness;
                if (runningSum >= randomValue)
                {
                    selected.Add(chromosome);
                    break;
                }
            }
        }

        Console.WriteLine($"Selected {selected.Count} chromosomes from population of {population.Count}");
        return selected;
    }
} 