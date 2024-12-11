using UMAT_GEN_TTS.Core.Models;
using UMAT_GEN_TTS.Core.Interfaces;
using UMAT_GEN_TTS.Core.Validator;
using UMAT_GEN_TTS.Core.Debug;
using System.Diagnostics;

namespace UMAT_GEN_TTS.Core.GeneticAlgorithms;

public class TimetableGeneticAlgorithm
{
    private readonly PopulationManager _populationManager;
    private readonly ISelectionStrategy _selectionStrategy;
    private readonly ICrossoverStrategy _crossoverStrategy;
    private readonly IMutationStrategy _mutationStrategy;
    private readonly IFitnessCalculator _fitnessCalculator;
    private readonly TimetableValidator _validator;
    private readonly TimetableDebugger _debugger;
    private readonly Stopwatch _stopwatch = new();
    
    private readonly int _maxGenerations;
    private readonly double _targetFitness;
    private readonly int _stagnationLimit;
    private readonly double _convergenceThreshold;
    
    private int _currentGeneration;
    private double _bestFitness;
    private ValidationResult _lastValidation;
    private readonly ILogger<TimetableGeneticAlgorithm> _logger;
    private double _currentMutationRate;
    private int _generationsWithoutImprovement;

    public TimetableGeneticAlgorithm(
        PopulationManager populationManager,
        ISelectionStrategy selectionStrategy,
        ICrossoverStrategy crossoverStrategy,
        IMutationStrategy mutationStrategy,
        IFitnessCalculator fitnessCalculator,
        TimetableValidator validator,
        ILogger<TimetableGeneticAlgorithm> logger,
        TimetableDebugger debugger,
        int maxGenerations = 1000,
        double targetFitness = 0.95,
        int stagnationLimit = 20,
        double convergenceThreshold = 0.001)
    {
        _populationManager = populationManager;
        _selectionStrategy = selectionStrategy;
        _crossoverStrategy = crossoverStrategy;
        _mutationStrategy = mutationStrategy;
        _fitnessCalculator = fitnessCalculator;
        _validator = validator;
        _logger = logger;
        _debugger = debugger;
        
        _maxGenerations = maxGenerations;
        _targetFitness = targetFitness;
        _stagnationLimit = stagnationLimit;
        _convergenceThreshold = convergenceThreshold;
    }

    public async Task<Chromosome> Evolve(EvolutionParameters parameters)
    {
        _logger.LogInformation("GA: Starting evolution with {CourseCount} courses", 
            parameters.Courses.Count);
        _stopwatch.Start();
        _debugger.LogInitialization(parameters.Courses, parameters.AvailableRooms);

        var population = _populationManager.InitializePopulation(parameters.Courses);
        _bestFitness = population.Max(c => c.Fitness);
        
        _debugger.LogGenerationProgress(0, population, _bestFitness);

        var generation = 0;
        return await Task.Run(async () =>
        {
            while (!ShouldTerminate(population))
            {
                // Log EVERY generation
                _logger.LogInformation($"Generation {generation:D4} | " +
                    $"Best: {_bestFitness:F2} | " +
                    $"Avg: {population.Average(c => c.Fitness):F2} | " +
                    $"Mutation: {_currentMutationRate:F2} | " +
                    $"No Improvement: {_generationsWithoutImprovement}");

                // Selection and crossover
                var parents = _selectionStrategy.Select(population, population.Count / 2);
                var offspring = CreateOffspring(parents);
                
                // Adaptive mutation
                AdaptParameters(population);
                foreach (var child in offspring)
                {
                    _mutationStrategy.Mutate(child);
                    child.CalculateFitness(_fitnessCalculator);
                }
                
                // Population management
                _populationManager.RepairPopulation(offspring);
                population = _populationManager.ReplacePopulation(population, offspring);
                
                // Update statistics
                var currentBestFitness = population.Max(c => c.Fitness);
                if (currentBestFitness > _bestFitness)
                {
                    _bestFitness = currentBestFitness;
                    _generationsWithoutImprovement = 0;
                }
                else
                {
                    _generationsWithoutImprovement++;
                }

                // Enforce diversity if needed
                if (_generationsWithoutImprovement > 10)
                {
                    _populationManager.EnforceDiversity(population);
                }

                generation++;
                _currentGeneration = generation;
                
                // Validate best solution
                var currentBest = population.MaxBy(c => c.Fitness) 
                    ?? throw new InvalidOperationException("No valid solution found");
                _lastValidation = _validator.ValidateSolution(currentBest);
                
                if (_lastValidation.IsValid && _lastValidation.FinalFitness >= _targetFitness)
                {
                    _logger.LogInformation("Target fitness achieved with valid solution");
                    break;
                }

                _debugger.LogGenerationProgress(generation, population, _bestFitness);

                await Task.Delay(100);
            }

            _logger.LogInformation("=== Evolution Complete ===");
            _logger.LogInformation($"Final Generation: {generation}");
            _logger.LogInformation($"Best Fitness Achieved: {_bestFitness:F2}");
            
            var bestSolution = population.MaxBy(c => c.Fitness) 
                ?? throw new InvalidOperationException("No valid solution found");

            _stopwatch.Stop();
            _debugger.LogAlgorithmCompletion(bestSolution, _stopwatch.Elapsed);

            return bestSolution;
        });
    }

    private void AdaptParameters(List<Chromosome> population)
    {
        var diversity = _populationManager.GetStatistics(population).Diversity;
        
        if (_generationsWithoutImprovement > 10)
        {
            _currentMutationRate = Math.Min(_currentMutationRate * 1.5, 0.4);
        }
        else if (diversity < _convergenceThreshold)
        {
            _currentMutationRate = Math.Min(_currentMutationRate * 1.2, 0.3);
        }
        else
        {
            _currentMutationRate = _mutationStrategy.BaseMutationRate;
        }
        
        if (_mutationStrategy is AdaptiveMutation adaptiveMutation)
        {
            adaptiveMutation.SetCurrentRate(_currentMutationRate);
        }
    }

    private bool ShouldTerminate(List<Chromosome> population)
    {
        if (_currentGeneration >= _maxGenerations) return true;
        if (_bestFitness >= _targetFitness) return true;
        if (_generationsWithoutImprovement >= _stagnationLimit) return true;
        
        var populationDiversity = _populationManager.GetStatistics(population).Diversity;
        if (populationDiversity < _convergenceThreshold) return true;
        
        return false;
    }

    private List<Chromosome> CreateOffspring(List<Chromosome> parents)
    {
        var offspring = new List<Chromosome>();
        
        for (int i = 0; i < parents.Count - 1; i += 2)
        {
            var (child1, child2) = _crossoverStrategy.Crossover(parents[i], parents[i + 1]);
            offspring.Add(child1);
            offspring.Add(child2);
        }
        
        return offspring;
    }
}