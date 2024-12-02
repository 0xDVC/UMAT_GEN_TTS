using UMAT_GEN_TTS.Core.Models;

public class TestDataGenerator
{
    public static List<Course> GenerateSampleCourses(int count)
    {
        var courses = new List<Course>();
        for (int i = 1; i <= count; i++)
        {
            courses.Add(new Course(
                code: $"CSC{100 + i}",
                name: $"Sample Course {i}",
                studentCount: Random.Shared.Next(20, 100),
                requiresLab: Random.Shared.Next(2) == 1,
                weeklyHours: Random.Shared.Next(2, 4) * 2,
                mode: CourseMode.Physical,
                department: "Computer Science"
            ));
        }
        return courses;
    }

    public static List<Room> GenerateSampleRooms(int count)
    {
        var rooms = new List<Room>();
        for (int i = 1; i <= count; i++)
        {
            rooms.Add(new Room
            {
                Name = $"Room {i}",
                Capacity = Random.Shared.Next(30, 120),
                IsLab = i % 5 == 0
            });
        }
        return rooms;
    }

    public static List<TimeSlot> GenerateTimeSlots()
    {
        var slots = new List<TimeSlot>();
        var days = Enum.GetValues<DayOfWeek>().Where(d => d != DayOfWeek.Saturday && d != DayOfWeek.Sunday);
        
        var periodStarts = new TimeSpan[]
        {
            new(8, 0, 0),
            new(10, 0, 0),
            new(12, 0, 0),
            new(14, 0, 0),
            new(16, 0, 0)
        };

        int period = 1;
        foreach (var day in days)
        {
            foreach (var start in periodStarts)
            {
                slots.Add(new TimeSlot(
                    day: day,
                    startTime: start,
                    endTime: start.Add(new TimeSpan(2, 0, 0))
                ));
            }
        }
        
        return slots;
    }
} 