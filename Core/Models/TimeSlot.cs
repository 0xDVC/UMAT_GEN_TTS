namespace UMAT_GEN_TTS.Core.Models;

public class TimeSlot
{
    public DayOfWeek Day { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public TimeOfDay PreferredTime { get; set; }
    public int Period { get; set; }

    public TimeSlot(DayOfWeek day, TimeSpan startTime, TimeSpan endTime)
    {
        Day = day;
        StartTime = startTime;
        EndTime = endTime;
        Period = startTime.Hours - 7;  // Calculate period based on start time
    }

    public bool IsPreferred
    {
        get
        {
            var hour = StartTime.Hours;
            return PreferredTime switch
            {
                TimeOfDay.Morning => hour >= 8 && hour < 12,
                TimeOfDay.Afternoon => hour >= 12 && hour < 16,
                TimeOfDay.Evening => hour >= 16 && hour < 20,
                _ => false
            };
        }
    }

    public bool Overlaps(TimeSlot other)
    {
        if (Day != other.Day) return false;
        return StartTime < other.EndTime && other.StartTime < EndTime;
    }

    public override string ToString()
    {
        return $"{Day} {StartTime.Hours:D2}:{StartTime.Minutes:D2}-{EndTime.Hours:D2}:{EndTime.Minutes:D2}";
    }
}

public enum TimeOfDay
{
    Morning,
    Afternoon,
    Evening
} 