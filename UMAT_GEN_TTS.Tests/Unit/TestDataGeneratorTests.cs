namespace UMAT_GEN_TTS.Tests.Unit;
public class TestDataGeneratorTests
{
    public static void RunTest()
    {
        try
        {
            Console.WriteLine("Testing Course Generation:");
            var courses = TestDataGenerator.GenerateSampleCourses(5);
            Console.WriteLine($"Generated {courses.Count} courses:");
            foreach (var course in courses)
            {
                Console.WriteLine($"- Course ID: {course.Id}");
                Console.WriteLine($"  Code: {course.Code}");
                Console.WriteLine($"  Name: {course.Name}");
                Console.WriteLine($"  Students: {course.StudentCount}");
                Console.WriteLine($"  Lecturer ID: {course.LecturerId}");
                Console.WriteLine();
            }

            Console.WriteLine("\nTesting Room Generation:");
            var rooms = TestDataGenerator.GenerateSampleRooms(3);
            Console.WriteLine($"Generated {rooms.Count} rooms:");
            foreach (var room in rooms)
            {
                Console.WriteLine($"- Room ID: {room.Id}");
                Console.WriteLine($"  Name: {room.Name}");
                Console.WriteLine($"  Capacity: {room.Capacity}");
                Console.WriteLine($"  Is Lab: {room.IsLab}");
                Console.WriteLine();
            }

            Console.WriteLine("\nTesting TimeSlot Generation:");
            var timeSlots = TestDataGenerator.GenerateTimeSlots();
            Console.WriteLine($"Generated {timeSlots.Count} time slots:");
            foreach (var slot in timeSlots.Take(3)) // Show first 3 slots as sample
            {
                Console.WriteLine($"- Day: {slot.Day}");
                Console.WriteLine($"  Time: {slot.StartTime} - {slot.EndTime}");
                Console.WriteLine($"  Period: {slot.Period}");
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in test data generation: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}