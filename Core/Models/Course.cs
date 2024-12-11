using UMAT_GEN_TTS.Core.Models.Preferences;

namespace UMAT_GEN_TTS.Core.Models;

public class Course
{
    public Course()
    {
        ProgrammeYears = new List<Programme>();
        SubClasses = new List<SubClass>();
        Preferences = new CoursePreferences();
    }

    public int Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public CourseMode Mode { get; set; }
    public Department AssignedDepartment { get; set; } = null!;
    public Lecturer? Lecturer { get; set; }
    
    public int WeeklyHours { get; set; }
    public int SessionsPerWeek { get; set; }
    public int SessionDuration { get; set; }
    public bool RequiresLab { get; set; }
    public bool HasLabSessions { get; set; }
    public bool HasPractical { get; set; }
    
    private List<Programme> _programmeYears = new();
    public List<Programme> ProgrammeYears 
    { 
        get => _programmeYears;
        set => _programmeYears = value ?? new List<Programme>();
    }

    public List<SubClass> SubClasses { get; set; } = new();
    public CoursePreferences Preferences { get; set; } = new();

    public int MinimumYear => _programmeYears.Count > 0 
        ? _programmeYears.Min(py => py.Year) 
        : 1;

    public bool CanScheduleAt(TimeSlot slot, Room? room)
    {
        if (Mode == CourseMode.Virtual && room != null)
            return false;

        if (Mode != CourseMode.Virtual && room == null)
            return false;

        if (Preferences.DaysNotAvailable.Contains(slot.Day))
            return false;

        if (room != null)
        {
            if (!room.IsAvailableForCourse(this, slot))
                return false;

            if (RequiresLab && !room.IsLab)
                return false;
        }

        if (Preferences.RequiresConsecutiveSessions)
        {
            var endTime = slot.StartTime.Add(TimeSpan.FromHours(SessionDuration));
            if (endTime > slot.EndTime)
                return false;
        }

        return true;
    }
}

public enum CourseMode
{
    Regular,
    Virtual,
    Hybrid
}

