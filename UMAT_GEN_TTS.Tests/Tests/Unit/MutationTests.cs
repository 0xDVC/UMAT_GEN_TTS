using UMAT_GEN_TTS.Core.GeneticAlgorithms;
using UMAT_GEN_TTS.Core.Models;


namespace UMAT_GEN_TTS.Tests.Unit;

public class MutationTests
{
    public static void RunTest()
    {
        Console.WriteLine("Testing Mutation Strategy\n");

        try
        {
            var testData = GeneticAlgorithmTests.GenerateTestData();
            var chromosome = CreateTestChromosome(testData);

            Console.WriteLine("Original Chromosome:");
            PrintChromosome(chromosome);

            var mutationStrategy = new RandomMutation(mutationRate: 0.5);
            mutationStrategy.Mutate(chromosome);

            Console.WriteLine("\nMutated Chromosome:");
            PrintChromosome(chromosome);

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
        foreach (var gene in chromosome.Genes)
        {
            // Course and TimeSlot should never be null
            if (gene.Course == null || gene.TimeSlot == null)
            {
                throw new InvalidOperationException("Invalid gene after mutation: null components detected");
            }

            // Room can be null only for virtual courses
            if (gene.Room == null && gene.Course.Mode != CourseMode.Virtual)
            {
                throw new InvalidOperationException(
                    $"Invalid gene after mutation: Room is null for non-virtual course {gene.Course.Code}");
            }

            // Room should be null for virtual courses
            if (gene.Room != null && gene.Course.Mode == CourseMode.Virtual)
            {
                throw new InvalidOperationException(
                    $"Invalid gene after mutation: Room assigned to virtual course {gene.Course.Code}");
            }

            // Validate room capacity for non-virtual courses
            if (gene.Room != null && gene.Course.Mode != CourseMode.Virtual)
            {
                if (gene.Room.Capacity < gene.Course.StudentCount)
                {
                    throw new InvalidOperationException(
                        $"Invalid room assignment: Room {gene.Room.Name} capacity ({gene.Room.Capacity}) " +
                        $"is less than course {gene.Course.Code} student count ({gene.Course.StudentCount})");
                }
            }

            // Validate lab requirements
            if (gene.Course.RequiresLab && gene.Room != null && !gene.Room.IsLab)
            {
                throw new InvalidOperationException(
                    $"Invalid room assignment: Course {gene.Course.Code} requires lab but assigned to non-lab room {gene.Room.Name}");
            }
        }
    }

    private static void PrintChromosome(Chromosome chromosome)
    {
        foreach (var gene in chromosome.Genes)
        {
            var roomInfo = gene.Room != null 
                ? $"in {gene.Room.Name}" 
                : "(virtual - no room)";
            
            Console.WriteLine($"Course: {gene.Course.Code} {roomInfo} at {gene.TimeSlot}");
        }
    }

    private static Chromosome CreateTestChromosome(
        (List<Course> Courses, List<Room> Rooms, List<TimeSlot> TimeSlots) testData)
    {
        var chromosome = new Chromosome();
        var usedTimeSlots = new HashSet<TimeSlot>();

        foreach (var course in testData.Courses)
        {
            // Find unused time slot
            var availableSlot = testData.TimeSlots.First(ts => !usedTimeSlots.Contains(ts));
            usedTimeSlots.Add(availableSlot);

            // For virtual courses, don't assign a room
            if (course.Mode == CourseMode.Virtual)
            {
                chromosome.Genes.Add(new Gene(course, availableSlot, null));
                continue;
            }

            // For non-virtual courses, find suitable room
            var suitableRoom = testData.Rooms.FirstOrDefault(r => 
                r.Capacity >= course.StudentCount &&
                (!course.RequiresLab || r.IsLab));

            if (suitableRoom == null)
            {
                throw new InvalidOperationException(
                    $"No suitable room found for course {course.Code}");
            }

            chromosome.Genes.Add(new Gene(course, availableSlot, suitableRoom));
        }

        return chromosome;
    }
} 