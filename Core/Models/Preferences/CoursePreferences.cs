namespace UMAT_GEN_TTS.Core.Models.Preferences;

public class CoursePreferences
{
    public Dictionary<TimeSlot, double> PreferredTimeSlots { get; init; } = new();
    public Dictionary<Room, double> PreferredRooms { get; init; } = new();
    public List<DayOfWeek> PreferredDays { get; init; } = new();
    public List<DayOfWeek> DaysNotAvailable { get; init; } = new();
    public SessionType PreferredSessionType { get; init; }
    public SessionSpreadPreference SpreadPreference { get; init; }
    public bool RequiresConsecutiveSessions { get; init; }
}

public enum SessionSpreadPreference
{
    Consecutive,
    SpreadOut,
    NoPreference
}
