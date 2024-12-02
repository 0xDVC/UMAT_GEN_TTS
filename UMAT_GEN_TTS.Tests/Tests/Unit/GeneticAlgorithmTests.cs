using UMAT_GEN_TTS.Core.GeneticAlgorithms;
using UMAT_GEN_TTS.Core.Models;



namespace UMAT_GEN_TTS.Tests.Unit;

public class GeneticAlgorithmTests
{
    public static void RunTest()
    {
        Console.WriteLine("Testing Genetic Algorithm Components\n");

        try
        {
            var testData = GenerateTestData();

            // Test chromosome creation
            var chromosome = new Chromosome();
            foreach (var course in testData.Courses)
            {
                var room = testData.Rooms[Random.Shared.Next(testData.Rooms.Count)];
                var timeSlot = testData.TimeSlots[Random.Shared.Next(testData.TimeSlots.Count)];
                chromosome.Genes.Add(new Gene(course, timeSlot, room));
            }

            // Calculate and print fitness
            var fitnessCalculator = new FitnessCalculator();
            chromosome.CalculateFitness(fitnessCalculator);

            PrintChromosomeDetails(chromosome);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Test failed: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    public static (List<Course> Courses, List<Room> Rooms, List<TimeSlot> TimeSlots) GenerateTestData()
    {
        Console.WriteLine("Generating courses...");
        var courses = new List<Course>
        {
            new Course(
                code: "MATH101",
                name: "Engineering Mathematics I",
                studentCount: 120,
                requiresLab: false,
                weeklyHours: 4,
                mode: CourseMode.Virtual,
                department: "Mathematics",
                programmeYears: new List<ProgrammeYear>
                {
                    new("CSC", 1),
                    new("EEE", 1)
                }
            ),
            new Course(
                code: "CSC201",
                name: "Data Structures",
                studentCount: 60,
                requiresLab: true,
                weeklyHours: 4,
                mode: CourseMode.Physical,
                department: "Computer Science",
                programmeYears: new List<ProgrammeYear>
                {
                    new("CSC", 2)
                }
            ),
            new Course(
                code: "GEG101",
                name: "Technical Communication",
                studentCount: 90,
                requiresLab: false,
                weeklyHours: 2,
                mode: CourseMode.Hybrid,
                department: "General Studies",
                programmeYears: new List<ProgrammeYear>
                {
                    new("CSC", 2),
                    new("EEE", 3)
                }
            )
        };

        Console.WriteLine("Generating rooms...");
        var rooms = new List<Room>
        {
            new()
            {
                Name = "LT1",
                Capacity = 200,
                IsLab = false,
                Features = new() { "Projector", "Whiteboard" }
            },
            new()
            {
                Name = "LAB1",
                Capacity = 120,
                IsLab = true,
                Features = new() { "Computers", "Projector" }
            },
            new()
            {
                Name = "CR2",
                Capacity = 100,
                IsLab = false,
                Features = new() { "Whiteboard" }
            }
        };

        Console.WriteLine("Generating time slots...");
        var timeSlots = TestDataGenerator.GenerateTimeSlots();

        // Validate test data
        ValidateTestData(courses, rooms, timeSlots);

        Console.WriteLine($"Generated {courses.Count} courses, {rooms.Count} rooms, and {timeSlots.Count} time slots.");
        return (courses, rooms, timeSlots);
    }

    private static void ValidateTestData(List<Course> courses, List<Room> rooms, List<TimeSlot> timeSlots)
    {
        // Check if there are enough rooms with sufficient capacity
        foreach (var course in courses.Where(c => c.Mode != CourseMode.Virtual))
        {
            var suitableRooms = rooms.Where(r => 
                r.Capacity >= course.StudentCount && 
                (!course.RequiresLab || r.IsLab)).ToList();

            if (!suitableRooms.Any())
            {
                throw new InvalidOperationException(
                    $"No suitable rooms for course {course.Code} (Students: {course.StudentCount}, Lab: {course.RequiresLab})");
            }
        }

        // Check if there are enough time slots
        if (timeSlots.Count < courses.Count)
        {
            throw new InvalidOperationException(
                $"Not enough time slots ({timeSlots.Count}) for courses ({courses.Count})");
        }
    }

    public static void PrintChromosomeDetails(Chromosome chromosome)
    {
        Console.WriteLine("\nChromosome Details:");
        Console.WriteLine("------------------");
        foreach (var gene in chromosome.Genes)
        {
            Console.WriteLine($"\nCourse: {gene.Course.Code} ({gene.Course.Name})");
            Console.WriteLine($"Mode: {gene.Course.Mode}");
            Console.WriteLine($"Programs: {string.Join(", ", gene.Course.ProgrammeYears)}");
            Console.WriteLine($"Room: {gene.Room?.Name}{(gene.IsCompromised ? "*" : "")} " +
                            $"(Capacity: {gene.Room?.Capacity}, Students: {gene.Course.StudentCount})");
            Console.WriteLine($"Time: {gene.TimeSlot}");
        }
        Console.WriteLine($"\nFitness: {chromosome.Fitness:F4}");
    }

    public static Chromosome CreateAndValidateChromosome(List<Course> courses, List<Room> rooms, List<TimeSlot> timeSlots)
    {
        var chromosome = new Chromosome();
        
        // Sort courses by student count (descending) to handle larger classes first
        var sortedCourses = courses.OrderByDescending(c => c.StudentCount).ToList();
        
        foreach (var course in sortedCourses)
        {
            Room? selectedRoom = null; // Start with null for all courses
            
            // ONLY proceed with room selection if course is NOT virtual
            if (course.Mode != CourseMode.Virtual)
            {
                // Find suitable rooms considering:
                // 1. Capacity (can fit the class with some flexibility)
                // 2. Lab requirements
                // 3. Prefer rooms that aren't too oversized
                var suitableRooms = rooms
                    .Where(r => r.Capacity >= course.StudentCount && // Must fit the class
                               r.Capacity <= course.StudentCount * 1.5 && // Not too big
                               (!course.RequiresLab || r.IsLab)) // Must be lab if required
                    .OrderBy(r => Math.Abs(r.Capacity - course.StudentCount)) // Best fit first
                    .ToList();

                // If no ideal room found, try again with relaxed size constraints
                if (!suitableRooms.Any())
                {
                    suitableRooms = rooms
                        .Where(r => r.Capacity >= course.StudentCount && // Must fit the class
                                   (!course.RequiresLab || r.IsLab)) // Must be lab if required
                        .OrderBy(r => r.Capacity) // Smallest suitable room first
                        .ToList();
                }

                if (suitableRooms.Any())
                {
                    selectedRoom = suitableRooms.First();
                }
                else
                {
                    Console.WriteLine($"Warning: No suitable room found for {course.Code}");
                }
            }
            // Virtual courses will keep null room assignment

            // Find a time slot that doesn't conflict with existing assignments
            TimeSlot? selectedTimeSlot = timeSlots
                .FirstOrDefault(slot => !chromosome.Genes.Any(g => 
                    g.Course.ProgrammeYears.Any(py1 => 
                        course.ProgrammeYears.Any(py2 => 
                            py1.ProgrammeCode == py2.ProgrammeCode && 
                            py1.Year == py2.Year)) && 
                    g.TimeSlot.Overlaps(slot)));

            // If no non-conflicting slot found, take any slot
            selectedTimeSlot ??= timeSlots[Random.Shared.Next(timeSlots.Count)];

            // Double-check that virtual courses have no room
            if (course.Mode == CourseMode.Virtual)
            {
                selectedRoom = null;
            }

            chromosome.Genes.Add(new Gene(course, selectedTimeSlot, selectedRoom));
        }
        
        var fitnessCalculator = new FitnessCalculator();
        chromosome.CalculateFitness(fitnessCalculator);
        return chromosome;
    }
}