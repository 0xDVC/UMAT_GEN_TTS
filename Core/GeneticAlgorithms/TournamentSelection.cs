using UMAT_GEN_TTS.Core.Interfaces;

namespace UMAT_GEN_TTS.Core.GeneticAlgorithms;

public class TournamentSelection : ISelectionStrategy
{
    private readonly Random _random = new();
    private readonly int _tournamentSize;
    private readonly double _selectionPressure;  // Probability of selecting the best

    public TournamentSelection(int tournamentSize = 3, double selectionPressure = 0.75)
    {
        _tournamentSize = tournamentSize;
        _selectionPressure = selectionPressure;
    }

    public List<Chromosome> Select(List<Chromosome> population, int numberToSelect)
    {
        var selected = new List<Chromosome>();
        
        while (selected.Count < numberToSelect)
        {
            // Select tournament participants
            var tournament = new List<Chromosome>();
            for (int i = 0; i < _tournamentSize; i++)
            {
                var randomIndex = _random.Next(population.Count);
                tournament.Add(population[randomIndex]);
            }

            // Sort by fitness
            tournament.Sort((a, b) => b.Fitness.CompareTo(a.Fitness));

            // Select winner based on selection pressure
            var winner = _random.NextDouble() < _selectionPressure 
                ? tournament[0]  // Best
                : tournament[_random.Next(1, tournament.Count)];  // Random from others

            selected.Add(winner);
        }

        return selected;
    }
} 