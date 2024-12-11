namespace UMAT_GEN_TTS.Core.Models;
using UMAT_GEN_TTS.Core.Helpers;

public class TimeSlot
{
    public Guid Id { get; } = Guid.NewGuid();
    public DayOfWeek Day { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public SessionType Type { get; init; }
    public bool IsBreakTime { get; init; }

    public int Period { get; init; }

    // Helper methods
    public long StartTimeTicks => TimeHelper.ToTicks(StartTime);
    public long EndTimeTicks => TimeHelper.ToTicks(EndTime);

    // For deserialization
    public void SetStartTimeTicks(long ticks) => StartTime = TimeHelper.FromTicks(ticks);
    public void SetEndTimeTicks(long ticks) => EndTime = TimeHelper.FromTicks(ticks);

    public TimeSlot(DayOfWeek day, TimeSpan startTime, TimeSpan endTime, 
        SessionType type = SessionType.Lecture, bool isBreakTime = false)
    {
        Day = day;
        StartTime = startTime;
        EndTime = endTime;
        Type = type;
        IsBreakTime = isBreakTime;
    }

    public bool Overlaps(TimeSlot other) =>
        Day == other.Day && StartTime < other.EndTime && EndTime > other.StartTime;
}

public enum SessionType
{
    Lecture,
    Lab,
    Practical,
    Combined
} 