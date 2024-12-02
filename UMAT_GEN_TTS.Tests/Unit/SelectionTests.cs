using UMAT_GEN_TTS.Core.GeneticAlgorithms;

namespace UMAT_GEN_TTS.Tests.Unit;

public class SelectionTests
{
    public static void RunTest()
    {
        Console.WriteLine("Testing Selection Strategy\n");

        try
        {
            // 1. Create a population of chromosomes with different fitness values
            var population = CreateTestPopulation();
            
            // 2. Test the selection process
            var selectionStrategy = new RouletteWheelSelection();
            var selectedChromosomes = selectionStrategy.Select(population, 2);

            // 3. Print results
            Console.WriteLine("Original Population:");
            PrintPopulation(population);

            Console.WriteLine("\nSelected Chromosomes:");
            PrintPopulation(selectedChromosomes);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Selection test failed: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    private static List<Chromosome> CreateTestPopulation()
    {
        var population = new List<Chromosome>();
        var testData = GeneticAlgorithmTests.GenerateTestData();

        // Create 4 different chromosomes with varying fitness
        for (int i = 0; i < 4; i++)
        {
            var chromosome = GeneticAlgorithmTests.CreateAndValidateChromosome(
                testData.Courses, 
                testData.Rooms, 
                testData.TimeSlots
            );
            
            // Assign different fitness values (0.2, 0.4, 0.6, 0.8)
            chromosome.CalculateFitness(new DummyFitnessCalculator((i + 1) * 0.2));
            population.Add(chromosome);
        }

        return population;
    }

    private static void PrintPopulation(List<Chromosome> chromosomes)
    {
        foreach (var chromosome in chromosomes)
        {
            Console.WriteLine($"Chromosome Fitness: {chromosome.Fitness}");
            foreach (var gene in chromosome.Genes)
            {
                Console.WriteLine($"- Course: {gene.Course.Code} | Room: {gene.Room.Name} | Time: {gene.TimeSlot.Day} {gene.TimeSlot.StartTime}");
            }
            Console.WriteLine();
        }
    }
} 