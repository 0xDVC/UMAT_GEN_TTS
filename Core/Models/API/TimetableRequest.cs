using UMAT_GEN_TTS.Core.Configuration;

namespace UMAT_GEN_TTS.Core.Models.API;

public class TimetableRequest
{

    public List<Course> Courses { get; set; } = new();
    public List<Room> Rooms { get; set; } = new();
    public SystemConfiguration Configuration { get; set; } = new();
}

public class TimetableResponse
{
    public bool IsValid { get; set; }
    public double Fitness { get; set; }
    public Schedule Schedule { get; set; } = new();  // Changed from List<ScheduleEntry>
    public List<string> Violations { get; set; } = new();
    public Dictionary<string, double> Metrics { get; set; } = new();
}
public class ScheduleEntry
{
    public string CourseCode { get; set; }
    public string CourseName { get; set; }
    public string RoomName { get; set; }
    public int Day { get; set; }
    public string StartTime { get; set; }
    public string EndTime { get; set; }
    public string LecturerName { get; set; }
    public string Programme { get; set; }  // Added
    public int Year { get; set; }          // Added
}

public class Schedule
{
    public Dictionary<int, List<ScheduleEntry>> EntriesByDay { get; set; }

    public Schedule()
    {
        EntriesByDay = new Dictionary<int, List<ScheduleEntry>>();
    }
}
