using UMAT_GEN_TTS.Core.GeneticAlgorithms;

namespace UMAT_GEN_TTS.Tests.Unit;

public class TimetableGeneticAlgorithmTests
{
    public static void RunTest()
    {
        Console.WriteLine("Testing Timetable Genetic Algorithm\n");

        try
        {
            // 1. Create test data
            var testData = GeneticAlgorithmTests.GenerateTestData();

            // 2. Initialize genetic algorithm components
            var ga = new TimetableGeneticAlgorithm(
                new BasicFitnessCalculator(),
                new RouletteWheelSelection(),
                new SinglePointCrossover(),
                new RandomMutation(0.1),  // 10% mutation rate
                populationSize: 20,        // Smaller population for testing
                maxGenerations: 100,       // Fewer generations for testing
                targetFitness: 0.95
            );

            // 3. Initialize and run
            ga.Initialize(testData.Courses, testData.Rooms, testData.TimeSlots);
            var bestSolution = ga.Run();

            // 4. Print results
            Console.WriteLine("\nBest Solution Found:");
            Console.WriteLine("--------------------");
            GeneticAlgorithmTests.PrintChromosomeDetails(bestSolution);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Genetic Algorithm test failed: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
} 