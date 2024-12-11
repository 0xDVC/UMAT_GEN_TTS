namespace UMAT_GEN_TTS.Core.Models.Preferences;

public class LecturerPreferences
{
    public List<DayOfWeek> PreferredDays { get; set; } = new();
    public List<DayOfWeek> UnavailableDays { get; set; } = new();
    public List<TimeRange> PreferredTimes { get; set; } = new();
    public List<TimeRange> UnavailableTimes { get; set; } = new();
    public bool PreferConsecutiveSessions { get; set; }
    public int PreferredMaxDailyHours { get; set; }
}

public class TimeRange
{
    public TimeSpan Start { get; set; }
    public TimeSpan End { get; set; }
}
