using UMAT_GEN_TTS.Core.GeneticAlgorithms;

namespace UMAT_GEN_TTS.Tests.Unit;
public class CrossoverTests
{
    public static void RunTest()
    {
        Console.WriteLine("Testing Crossover Strategy\n");

        try
        {
            // 1. Create parent chromosomes
            var testData = GeneticAlgorithmTests.GenerateTestData();
            var parent1 = GeneticAlgorithmTests.CreateAndValidateChromosome(testData.Courses, testData.Rooms, testData.TimeSlots);
            var parent2 = GeneticAlgorithmTests.CreateAndValidateChromosome(testData.Courses, testData.Rooms, testData.TimeSlots);

            // 2. Perform crossover
            var crossoverStrategy = new SinglePointCrossover();
            var (offspring1, offspring2) = crossoverStrategy.Crossover(parent1, parent2);

            // 3. Print results
            Console.WriteLine("Parent 1:");
            GeneticAlgorithmTests.PrintChromosomeDetails(parent1);

            Console.WriteLine("\nParent 2:");
            GeneticAlgorithmTests.PrintChromosomeDetails(parent2);

            Console.WriteLine("\nOffspring 1:");
            GeneticAlgorithmTests.PrintChromosomeDetails(offspring1);

            Console.WriteLine("\nOffspring 2:");
            GeneticAlgorithmTests.PrintChromosomeDetails(offspring2);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Crossover test failed: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
} 