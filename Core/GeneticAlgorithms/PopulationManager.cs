using UMAT_GEN_TTS.Core.Models;
using UMAT_GEN_TTS.Core.Interfaces;

namespace UMAT_GEN_TTS.Core.GeneticAlgorithms;

public class PopulationManager
{
    public int PopulationSize => _populationSize;
    private readonly Random _random = new();
    private readonly IFitnessCalculator _fitnessCalculator;
    private readonly int _populationSize;
    private readonly int _eliteCount;
    private readonly double _diversityThreshold;
    private readonly List<Room> _availableRooms;
    private readonly List<TimeSlot> _availableTimeSlots;
    private readonly List<Course> _courses;
    private int _currentPopulationSize;
    private readonly int _maxPopulationSize = 200;
    private readonly int _minPopulationSize = 50;
    private readonly double _growthFactor = 1.2;
    private int _currentGeneration;

    private double CalculatePopulationDiversity(List<Chromosome> population)
    {
        var averageSimilarity = 0.0;
        var comparisons = 0;

        for (int i = 0; i < population.Count - 1; i++)
        {
            for (int j = i + 1; j < population.Count; j++)
            {
                averageSimilarity += CalculateSimilarity(population[i], population[j]);
                comparisons++;
            }
        }

        return 1 - (averageSimilarity / comparisons); // Higher value = more diverse
    }

    // Update constructor
    public PopulationManager(
        IFitnessCalculator fitnessCalculator,
        List<Course> courses,
        int populationSize = 100,
        double elitePercentage = 0.1,
        double diversityThreshold = 0.1,
        List<Room>? rooms = null,
        List<TimeSlot>? timeSlots = null)
    {
        _courses = courses;
        _fitnessCalculator = fitnessCalculator;
        _populationSize = populationSize;
        _currentPopulationSize = populationSize;
        _eliteCount = (int)(populationSize * elitePercentage);
        _diversityThreshold = diversityThreshold;
        _availableRooms = rooms ?? new();
        _availableTimeSlots = timeSlots ?? new();
    }

    public List<Chromosome> InitializePopulation(List<Course> courses)
    {
        var population = new List<Chromosome>();

        for (int i = 0; i < _populationSize; i++)
        {
            var chromosome = GenerateValidChromosome(courses);
            chromosome.CalculateFitness(_fitnessCalculator);
            population.Add(chromosome);
        }

        return population;
    }

    public List<Chromosome> SelectElites(List<Chromosome> population)
    {
        return population
            .OrderByDescending(c => c.Fitness)
            .Take(_eliteCount)
            .ToList();
    }

    public bool IsDiverse(List<Chromosome> population)
    {
        if (population.Count < 2) return true;

        var averageFitness = population.Average(c => c.Fitness);
        var standardDeviation = Math.Sqrt(
            population.Average(c => Math.Pow(c.Fitness - averageFitness, 2)));

        return standardDeviation > _diversityThreshold;
    }

    private Chromosome GenerateValidChromosome(List<Course> courses)
    {
        var genes = new List<Gene>();
        foreach (var course in courses)
        {
            var timeSlot = GetValidTimeSlot(course, genes);
            var room = course.Mode == CourseMode.Virtual ? null :
                      GetValidRoom(course, timeSlot, genes);

            genes.Add(new Gene(course, timeSlot, room));
        }
        return new Chromosome(genes);
    }

    private TimeSlot GetValidTimeSlot(Course course, List<Gene> existingGenes)
    {
        if (!_availableTimeSlots.Any()) 
            throw new InvalidOperationException("No time slots available");

        var validSlots = _availableTimeSlots
            .Where(ts => !course.Preferences.DaysNotAvailable.Contains(ts.Day))
            .Where(ts => !HasTimeConflict(ts, course, existingGenes))
            .ToList();

        return validSlots.Any() 
            ? validSlots[_random.Next(validSlots.Count)]
            : _availableTimeSlots[_random.Next(_availableTimeSlots.Count)];
    }

    private Room? GetValidRoom(Course course, TimeSlot timeSlot, List<Gene> existingGenes)
    {
        if (!_availableRooms.Any()) 
            return null;

        var validRooms = _availableRooms
            .Where(r => r.MaxCapacity >= course.StudentCount)
            .Where(r => !course.RequiresLab || r.IsLab)
            .Where(r => !HasRoomConflict(r, timeSlot, existingGenes))
            .ToList();

        return validRooms.Any()
            ? validRooms[_random.Next(validRooms.Count)]
            : null;
    }

    private bool HasTimeConflict(TimeSlot slot, Course course, List<Gene> genes)
    {
        return genes.Any(g =>
            g.TimeSlot.Overlaps(slot) &&
            (g.Course.Lecturer == course.Lecturer ||
             g.Course.ProgrammeYears.Any(py1 =>
                 course.ProgrammeYears.Any(py2 =>
                     py1.Code == py2.Code && py1.Year == py2.Year))));
    }

    private bool HasRoomConflict(Room room, TimeSlot slot, List<Gene> genes)
    {
        return genes.Any(g =>
            g.Room == room && g.TimeSlot.Overlaps(slot));
    }

    public void EnforceDiversity(List<Chromosome> population)
    {
        var diversityThreshold = 0.7;
        var similarityGroups = new List<List<Chromosome>>();
        
        // Group similar chromosomes
        foreach (var chromosome in population)
        {
            var added = false;
            foreach (var group in similarityGroups)
            {
                if (CalculateSimilarity(chromosome, group[0]) > diversityThreshold)
                {
                    group.Add(chromosome);
                    added = true;
                    break;
                }
            }
            if (!added)
            {
                similarityGroups.Add(new List<Chromosome> { chromosome });
            }
        }

        // Keep best from each group, regenerate others
        foreach (var group in similarityGroups.Where(g => g.Count > 1))
        {
            var best = group.OrderByDescending(c => c.Fitness).First();
            foreach (var chromosome in group.Skip(1))
            {
                var index = population.IndexOf(chromosome);
                population[index] = GenerateValidChromosome(_courses);
                population[index].CalculateFitness(_fitnessCalculator);
            }
        }
    }

    public void AdaptPopulationSize(List<Chromosome> population, double convergenceRate)
    {
        if (convergenceRate > 0.8)
        {
            _currentPopulationSize = Math.Min(_maxPopulationSize,
                (int)(_currentPopulationSize * _growthFactor));
        }
        else if (convergenceRate < 0.2)
        {
            _currentPopulationSize = Math.Max(_minPopulationSize,
                (int)(_currentPopulationSize * (2 - _growthFactor)));
        }
    }

    private double CalculateSimilarity(Chromosome a, Chromosome b)
    {
        int matchingGenes = 0;
        for (int i = 0; i < a.Genes.Count; i++)
        {
            if (a.Genes[i].TimeSlot == b.Genes[i].TimeSlot &&
                a.Genes[i].Room == b.Genes[i].Room)
            {
                matchingGenes++;
            }
        }
        return (double)matchingGenes / a.Genes.Count;
    }

    public PopulationStats GetStatistics(List<Chromosome> population)
    {
        return new PopulationStats
        {
            AverageFitness = population.Average(c => c.Fitness),
            BestFitness = population.Max(c => c.Fitness),
            WorstFitness = population.Min(c => c.Fitness),
            Diversity = CalculatePopulationDiversity(population),
            GenerationNumber = _currentGeneration++
        };
    }

    public List<Chromosome> ReplacePopulation(
        List<Chromosome> currentPopulation,
        List<Chromosome> offspring)
    {
        var elites = SelectElites(currentPopulation);
        var newPopulation = new List<Chromosome>(elites);  // Keep best solutions

        // Fill rest with offspring, prioritizing better fitness
        newPopulation.AddRange(
                offspring.OrderByDescending(c => c.Fitness)
                        .Take(_populationSize - elites.Count));

        return newPopulation;
    }

    private readonly Queue<double> _bestFitnessHistory = new(10);

    public bool IsStagnating()
    {
        if (_bestFitnessHistory.Count < 10) return false;

        var improvement = _bestFitnessHistory.Last() - _bestFitnessHistory.First();
        return Math.Abs(improvement) < 0.001;  // Threshold for stagnation
    }

    public void UpdateStagnationMetrics(List<Chromosome> population)
    {
        var bestFitness = population.Max(c => c.Fitness);
        if (_bestFitnessHistory.Count >= 10)
            _bestFitnessHistory.Dequeue();
        _bestFitnessHistory.Enqueue(bestFitness);
    }

    public void RepairPopulation(List<Chromosome> population)
    {
        foreach (var chromosome in population)
        {
            if (chromosome.Genes.Any(g => g.IsCompromised))
            {
                RepairChromosome(chromosome);
                chromosome.CalculateFitness(_fitnessCalculator);
            }
        }
    }

    private void RepairChromosome(Chromosome chromosome)
    {
        foreach (var gene in chromosome.Genes.Where(g => g.IsCompromised))
        {
            var timeSlot = GetValidTimeSlot(gene.Course, chromosome.Genes);
            var room = gene.Course.Mode == CourseMode.Virtual ? null :
                    GetValidRoom(gene.Course, timeSlot, chromosome.Genes);

            gene.TimeSlot = timeSlot;
            gene.Room = room;
            gene.IsCompromised = false;
        }
    }
}

public record PopulationStats
{
    public double AverageFitness { get; init; }
    public double BestFitness { get; init; }
    public double WorstFitness { get; init; }
    public double Diversity { get; init; }
    public int GenerationNumber { get; init; }
}