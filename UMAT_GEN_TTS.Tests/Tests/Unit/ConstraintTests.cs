using UMAT_GEN_TTS.Core.Constraints;
using UMAT_GEN_TTS.Core.GeneticAlgorithms;
using UMAT_GEN_TTS.Core.Models;
using UMAT_GEN_TTS.Core.Interfaces;

namespace UMAT_GEN_TTS.Tests.Unit;

public class ConstraintTests
{
    public static void RunTest()
    {
        Console.WriteLine("Testing Constraints\n");
        Console.WriteLine("----------------------------------------");

        try
        {
            var testData = GeneticAlgorithmTests.GenerateTestData();
            
            // Individual Constraint Tests
            TestRoomCapacityConstraint(testData);
            TestTimeSlotConflictConstraint(testData);
            TestLabRequirementConstraint(testData);
            TestCombinedClassConstraint(testData);
            TestFlexibleScheduleConstraint(testData);
            TestLunchBreakConstraint(testData);
            TestTimePreferenceConstraint(testData);
            TestConsecutiveLecturesConstraint(testData);
            TestDailyTeachingLoadConstraint(testData);
            TestProgrammeYearSpreadConstraint(testData);
            
            
            // Combined Constraint Test
            TestAllConstraintsTogether(testData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Constraint test failed: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    private static void TestRoomCapacityConstraint(
        (List<Course> Courses, List<Room> Rooms, List<TimeSlot> TimeSlots) testData)
    {
        Console.WriteLine("\nTesting Room Capacity Constraint:");
        Console.WriteLine("--------------------------------");
        
        var constraint = new RoomCapacityConstraint();
        var chromosome = new Chromosome();

        // Test case: Room too small for class
        var largeClass = new Course(
            "TEST101", 
            "Test Course", 
            150, 
            requiresLab: false,
            creditHours: 3,
            sessionsPerWeek: 2,
            hoursPerSession: 2,
            mode: CourseMode.Physical,
            department: "TEST",
            programmeYears: new List<ProgrammeYear> { new("CSC", 1) }
        );
        var smallRoom = testData.Rooms.First(r => r.Capacity < 150);
        
        chromosome.Genes.Add(new Gene(largeClass, testData.TimeSlots[0], smallRoom));

        var penalty = constraint.EvaluatePenalty(chromosome);
        Console.WriteLine($"Penalty: {penalty}");
        Console.WriteLine(constraint.GetViolationMessage(chromosome));
    }

    private static void TestTimeSlotConflictConstraint(
        (List<Course> Courses, List<Room> Rooms, List<TimeSlot> TimeSlots) testData)
    {
        Console.WriteLine("\nTesting Time Slot Conflict Constraint:");
        Console.WriteLine("------------------------------------");
        
        var constraint = new TimeSlotConflictConstraint();
        var chromosome = new Chromosome();

        // Create time conflict for same programme
        var sameTimeSlot = new TimeSlot(DayOfWeek.Monday, new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0));
        chromosome.Genes.Add(new Gene(testData.Courses[0], sameTimeSlot, testData.Rooms[0]));
        chromosome.Genes.Add(new Gene(testData.Courses[1], sameTimeSlot, testData.Rooms[1]));

        var penalty = constraint.EvaluatePenalty(chromosome);
        Console.WriteLine($"Penalty: {penalty}");
        Console.WriteLine(constraint.GetViolationMessage(chromosome));
    }

    private static void TestLabRequirementConstraint(
        (List<Course> Courses, List<Room> Rooms, List<TimeSlot> TimeSlots) testData)
    {
        Console.WriteLine("\nTesting Lab Requirement Constraint:");
        Console.WriteLine("----------------------------------");
        
        var constraint = new LabRequirementConstraint();
        var chromosome = new Chromosome();

        // Create a course that requires lab but assign non-lab room
        var labCourse = new Course(
            "CSC201", "Programming Lab", 60, 
            requiresLab: true,
            creditHours: 3,
            sessionsPerWeek: 2,
            hoursPerSession: 2,
            CourseMode.Physical,  // Should be Virtual or Hybrid for multi-department
            "CSC"
        );
        
        var nonLabRoom = testData.Rooms.First(r => !r.IsLab);
        chromosome.Genes.Add(new Gene(labCourse, testData.TimeSlots[0], nonLabRoom));

        var penalty = constraint.EvaluatePenalty(chromosome);
        Console.WriteLine($"Penalty: {penalty}");
        Console.WriteLine(constraint.GetViolationMessage(chromosome));
    }

    private static void TestLunchBreakConstraint(
        (List<Course> Courses, List<Room> Rooms, List<TimeSlot> TimeSlots) testData)
    {
        Console.WriteLine("\nTesting Lunch Break Constraint:");
        Console.WriteLine("------------------------------");
        
        var constraint = new LunchBreakConstraint();
        var chromosome = new Chromosome();

        // Create a course that runs through lunch time
        var lunchTimeCourse = new Course(
            "CSC201", "Programming", 60,
            requiresLab: false,
            creditHours: 3,
            sessionsPerWeek: 2,
            hoursPerSession: 2,
            CourseMode.Physical,
            "CSC"
        );
        
        var lunchTimeSlot = new TimeSlot(
            DayOfWeek.Monday,
            new TimeSpan(11, 30, 0),  // Starts before lunch
            new TimeSpan(13, 30, 0)   // Ends after lunch
        );
        
        chromosome.Genes.Add(new Gene(lunchTimeCourse, lunchTimeSlot, testData.Rooms[0]));

        var penalty = constraint.EvaluatePenalty(chromosome);
        Console.WriteLine($"Penalty: {penalty}");
        Console.WriteLine(constraint.GetViolationMessage(chromosome));
    }

    private static void TestCombinedClassConstraint(
        (List<Course> Courses, List<Room> Rooms, List<TimeSlot> TimeSlots) testData)
    {
        Console.WriteLine("\nTesting Combined Class Constraint:");
        Console.WriteLine("--------------------------------");
        
        var constraint = new CombinedClassConstraint();
        var chromosome = new Chromosome();

        // Create a multi-department course
        var combinedCourse = new Course(
            "ENG201", "Engineering Math", 200,
            requiresLab: false,
            creditHours: 3,
            sessionsPerWeek: 2,
            hoursPerSession: 2,
            CourseMode.Physical,  // Should be Virtual or Hybrid for multi-department
            "ENG",
            lecturerId: "PROF1",
            programmeYears: new List<ProgrammeYear> 
            { 
                new("CE", 2), 
                new("EL", 2) 
            }
        );
        
        // Assign to physical room (violation for multi-department course)
        chromosome.Genes.Add(new Gene(combinedCourse, testData.TimeSlots[0], testData.Rooms[0]));

        var penalty = constraint.EvaluatePenalty(chromosome);
        Console.WriteLine($"Penalty: {penalty}");
        Console.WriteLine(constraint.GetViolationMessage(chromosome));
    }

    private static void TestFlexibleScheduleConstraint(
        (List<Course> Courses, List<Room> Rooms, List<TimeSlot> TimeSlots) testData)
    {
        Console.WriteLine("\nTesting Flexible Schedule Constraint:");
        Console.WriteLine("------------------------------------");
        
        var constraint = new FlexibleScheduleConstraint();
        var chromosome = new Chromosome();

        // Create a course scheduled too early
        var earlyClass = new Course(
            "CSC201", "Programming", 60,
            requiresLab: false,
            creditHours: 3,
            sessionsPerWeek: 2,
            hoursPerSession: 2,
            CourseMode.Physical,
            "CSC"
        );
        
        var earlyTimeSlot = new TimeSlot(
            DayOfWeek.Monday,
            new TimeSpan(6, 0, 0),  // Too early (before 6:30)
            new TimeSpan(8, 0, 0)
        );
        
        chromosome.Genes.Add(new Gene(earlyClass, earlyTimeSlot, testData.Rooms[0]));

        var penalty = constraint.EvaluatePenalty(chromosome);
        Console.WriteLine($"Penalty: {penalty}");
        Console.WriteLine(constraint.GetViolationMessage(chromosome));
    }

    private static void TestTimePreferenceConstraint(
        (List<Course> Courses, List<Room> Rooms, List<TimeSlot> TimeSlots) testData)
    {
        Console.WriteLine("\nTesting Time Preference Constraint:");
        Console.WriteLine("---------------------------------");
        
        var constraint = new TimePreferenceConstraint();
        var chromosome = new Chromosome();

        // Create a course scheduled at non-preferred time (early morning)
        var earlyClass = new Course(
            "CSC201", "Programming", 60,
            requiresLab: false,
            creditHours: 3,
            sessionsPerWeek: 2,
            hoursPerSession: 2,
            mode: CourseMode.Physical,
            department: "CSC"
        );
        
        var earlyTimeSlot = new TimeSlot(
            DayOfWeek.Monday,
            new TimeSpan(7, 0, 0),  // Early morning
            new TimeSpan(9, 0, 0)
        );
        
        chromosome.Genes.Add(new Gene(earlyClass, earlyTimeSlot, testData.Rooms[0]));

        var penalty = constraint.EvaluatePenalty(chromosome);
        Console.WriteLine($"Penalty: {penalty}");
        Console.WriteLine(constraint.GetViolationMessage(chromosome));
    }

    private static void TestConsecutiveLecturesConstraint(
        (List<Course> Courses, List<Room> Rooms, List<TimeSlot> TimeSlots) testData)
    {
        Console.WriteLine("\nTesting Consecutive Lectures Constraint:");
        Console.WriteLine("---------------------------------------");
        
        var constraint = new ConsecutiveLecturesConstraint();
        var chromosome = new Chromosome();

        // Create courses with a gap between them
        var course1 = new Course(
            "CSC201", "Programming I", 60,
            requiresLab: false,
            creditHours: 3,
            sessionsPerWeek: 2,
            hoursPerSession: 2,
            mode: CourseMode.Physical,
            department: "CSC",
            programmeYears: new List<ProgrammeYear> { new("CSC", 2) }
        );

        var course2 = new Course(
            "CSC202", "Programming II", 60,
            requiresLab: false,
            creditHours: 3,
            sessionsPerWeek: 2,
            hoursPerSession: 2,
            mode: CourseMode.Physical,
            department: "CSC",
            programmeYears: new List<ProgrammeYear> { new("CSC", 2) }
        );

        // Add with 2-hour gap between classes
        chromosome.Genes.Add(new Gene(
            course1,
            new TimeSlot(DayOfWeek.Monday, new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0)),
            testData.Rooms[0]
        ));

        chromosome.Genes.Add(new Gene(
            course2,
            new TimeSlot(DayOfWeek.Monday, new TimeSpan(12, 0, 0), new TimeSpan(14, 0, 0)),
            testData.Rooms[1]
        ));

        var penalty = constraint.EvaluatePenalty(chromosome);
        Console.WriteLine($"Penalty: {penalty}");
        Console.WriteLine(constraint.GetViolationMessage(chromosome));
    }

    private static void TestDailyTeachingLoadConstraint(
        (List<Course> Courses, List<Room> Rooms, List<TimeSlot> TimeSlots) testData)
    {
        Console.WriteLine("\nTesting Daily Teaching Load Constraint:");
        Console.WriteLine("--------------------------------------");
        
        var constraint = new DailyTeachingLoadConstraint();
        var chromosome = new Chromosome();

        // Create multiple courses for same programme on same day
        var programme = new ProgrammeYear("CSC", 2);
        for (int i = 0; i < 4; i++)  // 4 two-hour courses = 8 hours (exceeds limit)
        {
            var course = new Course(
                $"CSC20{i}", $"Course {i}", 60,
                requiresLab: false,
                creditHours: 3,
                sessionsPerWeek: 2,
                hoursPerSession: 2,
                mode: CourseMode.Physical,
                department: "CSC",
                programmeYears: new List<ProgrammeYear> { programme }
            );

            chromosome.Genes.Add(new Gene(
                course,
                new TimeSlot(DayOfWeek.Monday, new TimeSpan(8 + i*2, 0, 0), new TimeSpan(10 + i*2, 0, 0)),
                testData.Rooms[0]
            ));
        }

        var penalty = constraint.EvaluatePenalty(chromosome);
        Console.WriteLine($"Penalty: {penalty}");
        Console.WriteLine(constraint.GetViolationMessage(chromosome));
    }

    private static void TestProgrammeYearSpreadConstraint(
        (List<Course> Courses, List<Room> Rooms, List<TimeSlot> TimeSlots) testData)
    {
        Console.WriteLine("\nTesting Programme Year Spread Constraint:");
        Console.WriteLine("----------------------------------------");
        
        var constraint = new ProgrammeYearSpreadConstraint();
        var chromosome = new Chromosome();

        // Create multiple courses for same programme year on same day
        var programme = new ProgrammeYear("CSC", 2);
        for (int i = 0; i < 3; i++)  // 3 courses on same day (exceeds recommended)
        {
            var course = new Course(
                $"CSC20{i}", $"Course {i}", 60,
                requiresLab: false,
                creditHours: 3,
                sessionsPerWeek: 2,
                hoursPerSession: 2,
                mode: CourseMode.Physical,
                department: "CSC",
                programmeYears: new List<ProgrammeYear> { programme }
            );

            chromosome.Genes.Add(new Gene(
                course,
                new TimeSlot(DayOfWeek.Monday, new TimeSpan(8 + i*2, 0, 0), new TimeSpan(10 + i*2, 0, 0)),
                testData.Rooms[0]
            ));
        }

        var penalty = constraint.EvaluatePenalty(chromosome);
        Console.WriteLine($"Penalty: {penalty}");
        Console.WriteLine(constraint.GetViolationMessage(chromosome));
    }

    // ... (similar individual tests for other constraints)

    private static void TestAllConstraintsTogether(
        (List<Course> Courses, List<Room> Rooms, List<TimeSlot> TimeSlots) testData)
    {
        Console.WriteLine("\nTesting All Constraints Together:");
        Console.WriteLine("--------------------------------");

        var chromosome = new Chromosome();
        var monday = DayOfWeek.Monday;

        // Create a complex scenario that might violate multiple constraints
        // 1. Combined class with room capacity issues
        var combinedClass = new Course(
            "ENG201", 
            "Engineering Math", 
            200, 
            requiresLab: false,
            creditHours: 3,
            sessionsPerWeek: 2,
            hoursPerSession: 2,
            mode: CourseMode.Physical,
            department: "ENG",
            programmeYears: new List<ProgrammeYear> 
            { 
                new("CE", 2), 
                new("EL", 2), 
                new("RN", 2) 
            }
        );

        // 2. Lab course in non-lab room
        var labCourse = new Course(
            "CSC202", 
            "Programming Lab", 
            60, 
            requiresLab: true,
            creditHours: 3,
            sessionsPerWeek: 2,
            hoursPerSession: 2,
            mode: CourseMode.Physical,
            department: "CSC",
            programmeYears: new List<ProgrammeYear> { new("CSC", 2) }
        );

        // Add genes that violate multiple constraints
        chromosome.Genes.Add(new Gene(
            combinedClass,
            new TimeSlot(monday, new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0)),
            testData.Rooms[0] // Potentially too small
        ));

        chromosome.Genes.Add(new Gene(
            labCourse,
            new TimeSlot(monday, new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0)), // Time conflict
            testData.Rooms.First(r => !r.IsLab) // Wrong room type
        ));

        // Test with constraint manager
        var constraintManager = new ConstraintManager();
        var fitness = constraintManager.CalculateFitness(chromosome);

        Console.WriteLine($"Overall Fitness: {fitness}");
        Console.WriteLine("\nIndividual Constraint Violations:");
        
        var constraints = new List<(string Name, IConstraint Constraint)>
        {
            ("Room Capacity", new RoomCapacityConstraint()),
            ("Time Slot Conflict", new TimeSlotConflictConstraint()),
            ("Lab Requirement", new LabRequirementConstraint()),
            ("Combined Class", new CombinedClassConstraint()),
            ("Flexible Schedule", new FlexibleScheduleConstraint()),
            ("Lunch Break", new LunchBreakConstraint()),
            ("Time Preference", new TimePreferenceConstraint()),
            ("Consecutive Lectures", new ConsecutiveLecturesConstraint()),
            ("Daily Teaching Load", new DailyTeachingLoadConstraint()),
            ("Programme Year Spread", new ProgrammeYearSpreadConstraint())
        };

        foreach (var (name, constraint) in constraints)
        {
            var penalty = constraint.EvaluatePenalty(chromosome);
            Console.WriteLine($"\n{name} Constraint:");
            Console.WriteLine($"Penalty: {penalty}");
            Console.WriteLine(constraint.GetViolationMessage(chromosome));
        }
    }
}