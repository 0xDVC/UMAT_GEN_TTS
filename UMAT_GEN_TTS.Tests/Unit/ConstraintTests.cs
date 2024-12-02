using UMAT_GEN_TTS.Core.Constraints;
using UMAT_GEN_TTS.Core.GeneticAlgorithms;
using UMAT_GEN_TTS.Core.Models;

namespace UMAT_GEN_TTS.Tests.Unit;

public class ConstraintTests
{
    public static void RunTest()
    {
        Console.WriteLine("Testing Constraint System\n");
        Console.WriteLine("----------------------------------------");

        try
        {
            // Test each constraint type
            TestHardConstraints();
            TestSoftConstraints();
            TestConstraintManager();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Constraint test failed: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    private static void TestHardConstraints()
    {
        Console.WriteLine("\nTesting Hard Constraints:");
        Console.WriteLine("------------------------");

        var testData = GeneticAlgorithmTests.GenerateTestData();

        // 1. Test TimeSlotConflictConstraint
        Console.WriteLine("\n1. Time Slot Conflict Constraint:");
        var timeSlotConstraint = new TimeSlotConflictConstraint();

        // Create chromosome with time conflict
        var conflictChromosome = new Chromosome();
        var sameTimeSlot = testData.TimeSlots[0];
        var room1 = testData.Rooms[0];
        var room2 = testData.Rooms[1];

        conflictChromosome.Genes.Add(new Gene(testData.Courses[0], sameTimeSlot, room1));
        conflictChromosome.Genes.Add(new Gene(testData.Courses[1], sameTimeSlot, room2));

        var timeSlotPenalty = timeSlotConstraint.EvaluatePenalty(conflictChromosome);
        Console.WriteLine($"Time slot conflict penalty: {timeSlotPenalty}");
        Console.WriteLine(timeSlotConstraint.GetViolationMessage(conflictChromosome));

        // 2. Test RoomConflictConstraint
        Console.WriteLine("\n2. Room Conflict Constraint:");
        var roomConstraint = new RoomConflictConstraint();

        // Create chromosome with room conflict
        var roomConflictChromosome = new Chromosome();
        var sameRoom = testData.Rooms[0];

        roomConflictChromosome.Genes.Add(new Gene(testData.Courses[0], sameTimeSlot, sameRoom));
        roomConflictChromosome.Genes.Add(new Gene(testData.Courses[1], sameTimeSlot, sameRoom));

        var roomPenalty = roomConstraint.EvaluatePenalty(roomConflictChromosome);
        Console.WriteLine($"Room conflict penalty: {roomPenalty}");
        Console.WriteLine(roomConstraint.GetViolationMessage(roomConflictChromosome));
    }

    private static void TestSoftConstraints()
    {
        Console.WriteLine("\nTesting Soft Constraints:");
        Console.WriteLine("------------------------");

        var testData = GeneticAlgorithmTests.GenerateTestData();

        // 1. Test LabPreferenceConstraint
        Console.WriteLine("\n1. Lab Preference Constraint:");
        var labConstraint = new LabPreferenceConstraint();

        // Create chromosome with lab requirement violation
        var labViolationChromosome = new Chromosome();
        var nonLabRoom = testData.Rooms.First(r => !r.IsLab);
        var labCourse = testData.Courses.First(c => c.RequiresLab);

        labViolationChromosome.Genes.Add(new Gene(labCourse, testData.TimeSlots[0], nonLabRoom));

        var labPenalty = labConstraint.EvaluatePenalty(labViolationChromosome);
        Console.WriteLine($"Lab preference penalty: {labPenalty}");
        Console.WriteLine(labConstraint.GetViolationMessage(labViolationChromosome));

        // 2. Test TimePreferenceConstraint
        Console.WriteLine("\n2. Time Preference Constraint:");
        var timePreferenceConstraint = new TimePreferenceConstraint();

        // Create chromosome with non-preferred time slots
        var timePreferenceChromosome = new Chromosome();
        var afternoonSlot = testData.TimeSlots.First(ts => ts.StartTime.Hours >= 12);
        var labCourseInAfternoon = new Gene(labCourse, afternoonSlot, testData.Rooms[0]);

        timePreferenceChromosome.Genes.Add(labCourseInAfternoon);

        var timePreferencePenalty = timePreferenceConstraint.EvaluatePenalty(timePreferenceChromosome);
        Console.WriteLine($"Time preference penalty: {timePreferencePenalty}");
        Console.WriteLine(timePreferenceConstraint.GetViolationMessage(timePreferenceChromosome));
    }

    private static void TestConstraintManager()
    {
        Console.WriteLine("\nTesting Constraint Manager:");
        Console.WriteLine("-------------------------");

        var testData = GeneticAlgorithmTests.GenerateTestData();
        var constraintManager = new ConstraintManager();

        // 1. Test with perfect chromosome
        var perfectChromosome = CreatePerfectChromosome(testData);
        var perfectFitness = constraintManager.EvaluateFitness(perfectChromosome);
        Console.WriteLine($"\n1. Perfect chromosome fitness: {perfectFitness}");

        // 2. Test with violated constraints
        var violatedChromosome = CreateViolatedChromosome(testData);
        var violatedFitness = constraintManager.EvaluateFitness(violatedChromosome);
        Console.WriteLine($"\n2. Violated chromosome fitness: {violatedFitness}");
    }

    private static Chromosome CreatePerfectChromosome(
    (List<Course> Courses, List<Room> Rooms, List<TimeSlot> TimeSlots) testData)
    {
        var chromosome = new Chromosome();

        // Assign each course to different time slots and best available rooms
        for (int i = 0; i < testData.Courses.Count; i++)
        {
            var course = testData.Courses[i];

            // Try to find ideal room first
            var idealRooms = testData.Rooms.Where(r =>
                r.Capacity >= course.StudentCount &&
                r.IsLab == course.RequiresLab).ToList();

            // If no ideal room exists, fall back to rooms with sufficient capacity
            var availableRooms = idealRooms.Any()
                ? idealRooms
                : testData.Rooms.Where(r => r.Capacity >= course.StudentCount).ToList();

            if (!availableRooms.Any())
            {
                throw new InvalidOperationException(
                    $"No suitable room (even with relaxed constraints) for course {course.Code}");
            }

            var room = availableRooms.First();
            var timeSlot = testData.TimeSlots[i];

            chromosome.Genes.Add(new Gene(course, timeSlot, room));
        }

        return chromosome;
    }

    private static Chromosome CreateViolatedChromosome(
        (List<Course> Courses, List<Room> Rooms, List<TimeSlot> TimeSlots) testData)
    {
        var chromosome = new Chromosome();
        var sameTimeSlot = testData.TimeSlots[0];
        var sameRoom = testData.Rooms[0];

        // Create multiple violations
        foreach (var course in testData.Courses)
        {
            chromosome.Genes.Add(new Gene(course, sameTimeSlot, sameRoom));
        }

        return chromosome;
    }
}