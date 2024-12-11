namespace UMAT_GEN_TTS.Core.Models;

public class Lecturer
{
    public Guid Id { get; } = Guid.NewGuid();
    public string StaffId { get; init; }
    public string Name { get; init; }
    public Department Department { get; init; }
    public List<Course> AssignedCourses { get; init; } = new();
    public List<TimeSlot> PreferredTimeSlots { get; init; } = new();
    public List<DayOfWeek> PreferredDays { get; init; } = new();
    public List<DayOfWeek> DaysNotAvailable { get; init; } = new();
    public int MaxDailyHours { get; init; } = 6;
    public int MaxWeeklyHours { get; init; } = 18;
    public bool IsActive { get; init; } = true;
}