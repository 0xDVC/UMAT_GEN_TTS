using UMAT_GEN_TTS.Core.GeneticAlgorithms;


namespace UMAT_GEN_TTS.Tests.Unit;

public class MutationTests
{
    public static void RunTest()
    {
        Console.WriteLine("Testing Mutation Strategy\n");

        try
        {
            // 1. Create a chromosome to mutate
            var testData = GeneticAlgorithmTests.GenerateTestData();
            var chromosome = GeneticAlgorithmTests.CreateAndValidateChromosome(
                testData.Courses, 
                testData.Rooms, 
                testData.TimeSlots
            );

            // 2. Store original state
            Console.WriteLine("Original Chromosome:");
            GeneticAlgorithmTests.PrintChromosomeDetails(chromosome);

            // 3. Perform mutation
            var mutationStrategy = new RandomMutation(0.5); // 50% mutation rate
            mutationStrategy.Mutate(chromosome);

            // 4. Print results
            Console.WriteLine("\nAfter Mutation:");
            GeneticAlgorithmTests.PrintChromosomeDetails(chromosome);

            // 5. Validate mutation results
            ValidateMutation(chromosome);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Mutation test failed: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    private static void ValidateMutation(Chromosome chromosome)
    {
        // Verify that the chromosome is still valid after mutation
        foreach (var gene in chromosome.Genes)
        {
            if (gene.Course == null || gene.Room == null || gene.TimeSlot == null)
                throw new InvalidOperationException("Invalid gene after mutation: null components detected");

            if (gene.Room.Capacity < gene.Course.StudentCount)
                throw new InvalidOperationException($"Invalid room assignment for course {gene.Course.Code}: insufficient capacity");

            if (gene.Course.RequiresLab && !gene.Room.IsLab)
                throw new InvalidOperationException($"Invalid room assignment for course {gene.Course.Code}: lab required but not assigned");
        }

        Console.WriteLine("\nMutation validation passed successfully!");
    }
}