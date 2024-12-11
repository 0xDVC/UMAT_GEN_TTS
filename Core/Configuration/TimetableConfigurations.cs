namespace UMAT_GEN_TTS.Core.Configuration;
using UMAT_GEN_TTS.Core.Models.Preferences;
public class SystemConfiguration
{
    public TimeConfiguration TimeSettings { get; set; } = new();
    public RoomConfiguration RoomSettings { get; set; } = new();
    public CourseConfiguration CourseSettings { get; set; } = new();
    public LecturerConfiguration LecturerSettings { get; set; } = new();
    public ConstraintConfiguration ConstraintSettings { get; set; } = new();
    public AlgorithmConfiguration AlgorithmSettings { get; set; } = new();
}

public class TimeConfiguration
{
    private Dictionary<int, DayConfig> _numericDayConfigs = new();
    public Dictionary<DayOfWeek, DayConfig> DayConfigurations 
    { 
        get => _numericDayConfigs.ToDictionary(
            kvp => (DayOfWeek)kvp.Key, 
            kvp => kvp.Value);
        set => _numericDayConfigs = value.ToDictionary(
            kvp => (int)kvp.Key, 
            kvp => kvp.Value);
    }
    public TimeSpan MinSessionDuration { get; set; } = TimeSpan.FromHours(1);
    public TimeSpan MaxSessionDuration { get; set; } = TimeSpan.FromHours(4);
    public bool AllowWeekends { get; set; } = false;

    public DayConfig GetDayConfig(DayOfWeek day)
    {
        return DayConfigurations.GetValueOrDefault(day, DayConfigurations[DayOfWeek.Monday]);
    }
}

public class DayConfig
{
    public TickTime StartTime { get; set; } = new();
    public TickTime EndTime { get; set; } = new();
    public List<Break> Breaks { get; set; } = new();
    public List<TickTime> AllowedDurations { get; set; } = new();
}

public class Break
{
    public TickTime StartTime { get; set; } = new();
    public TickTime Duration { get; set; } = new();
}

public class TickTime
{
    public long Ticks { get; set; }

    public TimeSpan ToTimeSpan() => TimeSpan.FromTicks(Ticks);
    
    public static implicit operator TimeSpan(TickTime tickTime) 
        => TimeSpan.FromTicks(tickTime.Ticks);
}

public class RoomConfiguration
{
    public int MinRoomCapacity { get; set; } = 20;
    public int MaxRoomCapacity { get; set; } = 200;
    public bool AllowSharedRooms { get; set; } = true;
    public bool RequireLabEquipment { get; set; } = true;
    public Dictionary<string, List<string>> DepartmentRooms { get; set; } = new();
}

public class CourseConfiguration
{
    public int MinStudentsPerClass { get; set; } = 5;
    public int MaxStudentsPerClass { get; set; } = 120;
    public bool AutoSplitLargeClasses { get; set; } = true;
    public bool AllowVirtualSessions { get; set; } = true;
    public Dictionary<string, CourseRequirements> CourseRequirements { get; set; } = new();
}

public class CourseRequirements
{
    public bool RequiresLab { get; set; }
    public bool HasLabSessions { get; set; }
    public int MinimumCapacity { get; set; }
    public List<string> RequiredEquipment { get; set; } = new();
}

public class LecturerConfiguration
{
    public Dictionary<string, LecturerPreferences> LecturerPreferences { get; set; } = new();
    public int MaxDailyHours { get; set; } = 6;
    public int MaxWeeklyHours { get; set; } = 24;
    public bool AllowConsecutiveSessions { get; set; } = true;
}

public class ConstraintConfiguration
{
    public Dictionary<string, double> HardConstraintWeights { get; set; } = new();
    public Dictionary<string, double> SoftConstraintWeights { get; set; } = new();
    public double MinAcceptableFitness { get; set; } = 0.85;
    public bool StrictConstraintMode { get; set; } = true;
}

public class AlgorithmConfiguration
{
    public int MaxGenerations { get; set; } = 1000;
    public double TargetFitness { get; set; } = 0.95;
    public int PopulationSize { get; set; } = 100;
    public double ElitismRate { get; set; } = 0.1;
    public double MutationRate { get; set; } = 0.1;
    public int StagnationLimit { get; set; } = 20;
    public double CrossoverRate { get; set; } = 0.8;
    public int TournamentSize { get; set; } = 5;
} 