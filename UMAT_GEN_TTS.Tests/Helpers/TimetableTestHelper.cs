using UMAT_GEN_TTS.Core.Models;
using UMAT_GEN_TTS.Core.Models.Preferences;

public class TimetableTestHelper
{
    protected static class TestData 
    {
        public static List<Room> GetRooms()
        {
            return new List<Room>
            {
                CreateRoom("LH1", 100, RoomType.LectureHall),
                CreateRoom("LH2", 100, RoomType.LectureHall),
                CreateRoom("LH3", 100, RoomType.LectureHall),
                CreateRoom("LH5", 100, RoomType.LectureHall),
                CreateRoom("LH6", 100, RoomType.LectureHall),
                CreateRoom("LH7", 100, RoomType.LectureHall),
                CreateRoom("LH8", 100, RoomType.LectureHall),
                CreateRoom("LH9", 100, RoomType.LectureHall),
                CreateRoom("ED III", 80, RoomType.Laboratory),
                CreateRoom("MRT I", 60, RoomType.Laboratory),
                CreateRoom("MRT II", 60, RoomType.Laboratory),
                CreateRoom("MRT III", 60, RoomType.Laboratory),
                CreateRoom("CB1", 50, RoomType.ComputerLab),
                CreateRoom("CB2", 50, RoomType.ComputerLab),
                CreateRoom("CL1", 40, RoomType.LectureHall),
                CreateRoom("MS1", 40, RoomType.Laboratory),
                CreateRoom("MS2", 40, RoomType.Laboratory),
                CreateRoom("GE1", 60, RoomType.LectureHall),
                CreateRoom("FF3", 40, RoomType.LectureHall)
            };
        }

        public static List<Department> GetDepartments()
        {
            return new List<Department>
            {
                new() { Code = "MR", Name = "Mining" },
                new() { Code = "PE", Name = "Petroleum" },
                new() { Code = "GM", Name = "Geomatics" },
                new() { Code = "CY", Name = "Chemistry" },
                new() { Code = "MA", Name = "Mathematics" },
                new() { Code = "ES", Name = "Earth Science" },
                new() { Code = "NG", Name = "Engineering" },
                new() { Code = "GL", Name = "Geology" }
            };
        }

        public static List<Course> GetCourses()
        {
            var departments = GetDepartments();
            var courses = new List<Course>();

            // Mining courses
            AddCourse("MR 377", "Mining Engineering", 30, departments[0], false);
            AddCourse("MR 1B 151", "Mining Practice", 25, departments[0], true);
            AddCourse("MR 1A 153", "Mining Lab", 20, departments[0], true);

            // Petroleum courses  
            AddCourse("PE 153", "Petroleum Engineering", 35, departments[1], false);
            AddCourse("PE 3A 375", "Petroleum Lab", 20, departments[1], true);

            // Add more courses following the pattern in image...

            return courses;

            void AddCourse(string code, string name, int students, Department dept, bool isLab)
            {
                courses.Add(new Course
                {
                    Code = int.Parse(code.Replace(dept.Code, "").Replace(" ", "")),
                    Name = name,
                    StudentCount = students,
                    RequiresLab = isLab,
                    Mode = isLab ? CourseMode.Practical : CourseMode.Regular,
                    AssignedDepartment = dept,
                    WeeklyHours = 2,
                    SessionsPerWeek = 1,
                    Preferences = new CoursePreferences
                    {
                        PreferredTimeSlots = new Dictionary<TimeSlot, double>(),
                        PreferredRooms = new Dictionary<Room, double>()
                    }
                });
            }
        }

        public static List<TimeSlot> GetTimeSlots()
        {
            var slots = new List<TimeSlot>();
            var days = new[] { 
                DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, 
                DayOfWeek.Thursday, DayOfWeek.Friday 
            };

            // Based on image time slots: 7:00-8:00, 8:00-9:00, etc.
            var startTimes = new[] { 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 };

            foreach (var day in days)
            {
                foreach (var startHour in startTimes)
                {
                    slots.Add(new TimeSlot(
                        day,
                        TimeSpan.FromHours(startHour),
                        TimeSpan.FromHours(startHour + 1),
                        SessionType.Lecture,
                        true
                    ));
                }
            }

            return slots;
        }

        private static Room CreateRoom(string name, int capacity, RoomType type)
        {
            return new Room
            {
                Name = name,
                MaxCapacity = capacity,
                IsLab = type != RoomType.LectureHall,
                IsAvailable = true,
                Features = new List<string>(),
                Equipment = new List<string>()
            };
        }
    }

    protected enum RoomType
    {
        LectureHall,
        Laboratory,
        ComputerLab,
    }
} 