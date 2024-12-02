using UMAT_GEN_TTS.Core.GeneticAlgorithms;
using UMAT_GEN_TTS.Core.Models;

namespace UMAT_GEN_TTS.Tests.Unit;
public class FitnessCalculatorTests
{
    public static void RunTest()
    {
        Console.WriteLine("Testing Fitness Calculator\n");

        try
        {
            // Create test chromosomes with known conflicts
            var testData = GeneticAlgorithmTests.GenerateTestData();
            
            // 1. Create a chromosome with no conflicts
            var goodChromosome = CreateConflictFreeChromosome(testData);
            
            // 2. Create a chromosome with time slot conflicts
            var timeConflictChromosome = CreateTimeConflictChromosome(testData);
            
            // 3. Create a chromosome with room conflicts
            var roomConflictChromosome = CreateRoomConflictChromosome(testData);

            var calculator = new FitnessCalculator();

            // Test and print results
            Console.WriteLine("\nTesting different chromosome configurations:");
            Console.WriteLine("----------------------------------------");
            
            Console.WriteLine("\n1. Conflict-free chromosome:");
            Console.WriteLine("Expected: High fitness (close to 1.0)");
            Console.WriteLine($"Actual fitness: {calculator.Calculate(goodChromosome)}");
            PrintChromosomeDetails(goodChromosome);
            
            Console.WriteLine("\n2. Time conflict chromosome:");
            Console.WriteLine("Expected: Lower fitness (around 0.6-0.8 due to time slot conflicts)");
            Console.WriteLine($"Actual fitness: {calculator.Calculate(timeConflictChromosome)}");
            PrintChromosomeDetails(timeConflictChromosome);
            
            Console.WriteLine("\n3. Room conflict chromosome:");
            Console.WriteLine("Expected: Lower fitness (around 0.6-0.8 due to room conflicts)");
            Console.WriteLine($"Actual fitness: {calculator.Calculate(roomConflictChromosome)}");
            PrintChromosomeDetails(roomConflictChromosome);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fitness calculation test failed: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    private static Chromosome CreateConflictFreeChromosome(
        (List<Course> Courses, List<Room> Rooms, List<TimeSlot> TimeSlots) testData)
    {
        var chromosome = new Chromosome();
        
        // Ensure each course gets a different time slot and suitable room
        for (int i = 0; i < testData.Courses.Count; i++)
        {
            var course = testData.Courses[i];
            var room = testData.Rooms.First(r => 
                r.Capacity >= course.StudentCount && 
                r.IsLab == course.RequiresLab);
            var timeSlot = testData.TimeSlots[i]; // Different time slot for each

            chromosome.Genes.Add(new Gene(course, timeSlot, room));
        }

        return chromosome;
    }

    private static Chromosome CreateTimeConflictChromosome(
        (List<Course> Courses, List<Room> Rooms, List<TimeSlot> TimeSlots) testData)
    {
        var chromosome = new Chromosome();
        var sameTimeSlot = testData.TimeSlots[0]; // Use same time slot to create conflict

        // Assign same time slot to multiple courses
        foreach (var course in testData.Courses)
        {
            var room = testData.Rooms.First(r => 
                r.Capacity >= course.StudentCount && 
                r.IsLab == course.RequiresLab);

            chromosome.Genes.Add(new Gene(course, sameTimeSlot, room));
        }

        return chromosome;
    }

    private static Chromosome CreateRoomConflictChromosome(
        (List<Course> Courses, List<Room> Rooms, List<TimeSlot> TimeSlots) testData)
    {
        var chromosome = new Chromosome();
        var sameRoom = testData.Rooms[0]; // Use same room to create conflict
        var sameTimeSlot = testData.TimeSlots[0]; // And same time slot

        // Assign same room and time slot to multiple courses
        foreach (var course in testData.Courses)
        {
            chromosome.Genes.Add(new Gene(course, sameTimeSlot, sameRoom));
        }

        return chromosome;
    }

     private static void PrintChromosomeDetails(Chromosome chromosome)
    {
        Console.WriteLine("Chromosome details:");
        foreach (var gene in chromosome.Genes)
        {
            var roomInfo = gene.Room != null 
                ? $"in {gene.Room.Name} (Capacity: {gene.Room.Capacity}, Students: {gene.Course.StudentCount})"
                : "(virtual - no room)";

            Console.WriteLine($"\nCourse: {gene.Course.Code} ({gene.Course.Name})");
            Console.WriteLine($"Mode: {gene.Course.Mode}");
            Console.WriteLine($"Programs: {string.Join(", ", gene.Course.ProgrammeYears)}");
            Console.WriteLine($"Room: {roomInfo}");
            Console.WriteLine($"Time: {gene.TimeSlot}");
        }
    }
} 