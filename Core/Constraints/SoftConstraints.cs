using UMAT_GEN_TTS.Core.Interfaces;
using UMAT_GEN_TTS.Core.GeneticAlgorithms;
using UMAT_GEN_TTS.Core.Models;

namespace UMAT_GEN_TTS.Core.Constraints;

public class RoomEfficiencyConstraint : IConstraint
{
    public const double PENALTY = 0.1;
    private const double OPTIMAL_UTILIZATION_MIN = 0.7;
    private const double OPTIMAL_UTILIZATION_MAX = 0.9;

    public double EvaluatePenalty(Chromosome chromosome)
    {
        double penalty = 0;
        foreach (var gene in chromosome.Genes.Where(g => g.Room != null && g.Course != null))
        {
            if (gene.Course.Mode != CourseMode.Virtual)
            {
                double utilization = (double)gene.Course.StudentCount / gene.Room!.MaxCapacity;
                
                if (utilization < OPTIMAL_UTILIZATION_MIN)
                {
                    penalty += (OPTIMAL_UTILIZATION_MIN - utilization) * 0.5;
                }
                else if (utilization > OPTIMAL_UTILIZATION_MAX)
                {
                    penalty += (utilization - OPTIMAL_UTILIZATION_MAX) * 0.3;
                }
            }
        }
        return penalty * PENALTY;
    }

    public string GetViolationMessage(Chromosome chromosome)
    {
        var inefficientRooms = chromosome.Genes
            .Where(g => g.Room != null && g.Course != null && g.Course.Mode != CourseMode.Virtual)
            .Where(g => {
                double utilization = (double)g.Course.StudentCount / g.Room!.MaxCapacity;
                return utilization < OPTIMAL_UTILIZATION_MIN || utilization > OPTIMAL_UTILIZATION_MAX;
            })
            .Select(g => $"{g.Course.Code} in {g.Room!.Name} ({g.Course.StudentCount}/{g.Room.MaxCapacity} = {(double)g.Course.StudentCount / g.Room.MaxCapacity:P0})");
        
        return $"Room efficiency issues: {string.Join(", ", inefficientRooms)}";
    }
}

public class TimePreferenceConstraint : IConstraint
{
    public const double PENALTY = 0.05;
    private static readonly TimeSpan PREFERRED_START = new(9, 0, 0);
    private static readonly TimeSpan PREFERRED_END = new(16, 0, 0);

    public double EvaluatePenalty(Chromosome chromosome)
    {
        return chromosome.Genes.Count(gene => 
            gene.TimeSlot.StartTime < PREFERRED_START || 
            gene.TimeSlot.EndTime > PREFERRED_END) * PENALTY;
    }

    public string GetViolationMessage(Chromosome chromosome)
    {
        var violations = chromosome.Genes
            .Where(g => g.TimeSlot.StartTime < PREFERRED_START || g.TimeSlot.EndTime > PREFERRED_END)
            .Select(g => $"{g.Course.Code} at {g.TimeSlot}");
        return $"Time preference violations: {string.Join(", ", violations)}";
    }
}

public class ProgrammeYearSpreadConstraint : IConstraint
{
    public const double PENALTY = 0.2;
    private const int MAX_DAILY_SESSIONS = 3;
    private const int MIN_DAYS_SPREAD = 2;

    public double EvaluatePenalty(Chromosome chromosome)
    {
        var programmeGroups = chromosome.Genes
            .SelectMany(g => g.Course.ProgrammeYears.Select(py => 
                new { ProgrammeYear = py, Gene = g }))
            .GroupBy(x => new { x.ProgrammeYear.Code, x.ProgrammeYear.Year });

        double penalty = 0;
        foreach (var group in programmeGroups)
        {
            // Check daily session count
            var dailySessions = group
                .GroupBy(x => x.Gene.TimeSlot.Day)
                .Where(d => d.Count() > MAX_DAILY_SESSIONS);
            penalty += dailySessions.Count() * 0.5;

            // Check spread across week
            var uniqueDays = group.Select(x => x.Gene.TimeSlot.Day).Distinct().Count();
            if (uniqueDays < MIN_DAYS_SPREAD)
            {
                penalty += (MIN_DAYS_SPREAD - uniqueDays) * 0.5;
            }
        }
        return penalty * PENALTY;
    }

    public string GetViolationMessage(Chromosome chromosome)
    {
        var violations = new List<string>();
        var programmeGroups = chromosome.Genes
            .SelectMany(g => g.Course.ProgrammeYears.Select(py => 
                new { ProgrammeYear = py, Gene = g }))
            .GroupBy(x => new { x.ProgrammeYear.Code, x.ProgrammeYear.Year });

        foreach (var group in programmeGroups)
        {
            var dailyOverloads = group
                .GroupBy(x => x.Gene.TimeSlot.Day)
                .Where(d => d.Count() > MAX_DAILY_SESSIONS)
                .Select(d => $"{group.Key.Code} Year {group.Key.Year}: {d.Count()} sessions on {d.Key}");
            violations.AddRange(dailyOverloads);

            var uniqueDays = group.Select(x => x.Gene.TimeSlot.Day).Distinct().Count();
            if (uniqueDays < MIN_DAYS_SPREAD)
            {
                violations.Add($"{group.Key.Code} Year {group.Key.Year}: Spread across only {uniqueDays} days");
            }
        }

        return $"Programme spread violations: {string.Join(", ", violations)}";
    }
}

public class LabPreferenceConstraint : IConstraint
{
    public const double PENALTY = 0.05;

    public double EvaluatePenalty(Chromosome chromosome)
    {
        double penalty = 0;
        foreach (var gene in chromosome.Genes)
        {
            // Must be in lab if course requires it
            if (gene.Course.RequiresLab && (gene.Room == null || !gene.Room.IsLab))
            {
                penalty += 1.0;
            }
            // Should try to get lab if course has lab sessions
            else if (gene.Course.HasLabSessions && 
                     gene.Course.Mode != CourseMode.Virtual &&
                     (gene.Room == null || !gene.Room.IsLab))
            {
                penalty += 0.5; // Lower penalty for preference vs requirement
            }
        }
        return penalty * PENALTY;
    }

    public string GetViolationMessage(Chromosome chromosome)
    {
        var violations = chromosome.Genes
            .Where(g => (g.Course.RequiresLab || g.Course.HasLabSessions) &&
                       (g.Room == null || !g.Room.IsLab))
            .Select(g => $"{g.Course.Code} ({(g.Course.RequiresLab ? "Requires" : "Prefers")} lab)");

        return $"Lab assignment issues: {string.Join(", ", violations)}";
    }
}

public class ConsecutiveLecturesConstraint : IConstraint
{
    public const double PENALTY = 0.05;

    public double EvaluatePenalty(Chromosome chromosome)
    {
        double penalty = 0;
        var programmeGroups = chromosome.Genes
            .GroupBy(g => g.Course.ProgrammeYears.First()); // Simplification: using first programme

        foreach (var group in programmeGroups)
        {
            var dayGroups = group
                .GroupBy(g => g.TimeSlot.Day)
                .Where(g => g.Count() > 1);

            foreach (var dayGroup in dayGroups)
            {
                // Check for gaps between lectures
                var sortedSlots = dayGroup.OrderBy(g => g.TimeSlot.StartTime).ToList();
                for (int i = 0; i < sortedSlots.Count - 1; i++)
                {
                    var gap = sortedSlots[i + 1].TimeSlot.StartTime - sortedSlots[i].TimeSlot.EndTime;
                    if (gap.TotalHours > 1) // More than 1-hour gap
                    {
                        penalty += 0.1;
                    }
                }
            }
        }
        return penalty * PENALTY;
    }

    public string GetViolationMessage(Chromosome chromosome)
    {
        var violations = new List<string>();
        // Implementation similar to EvaluatePenalty but collecting messages
        return $"Consecutive lecture violations: {string.Join(", ", violations)}";
    }
}

public class DailyTeachingLoadConstraint : IConstraint
{
    public const double PENALTY = 0.05;
    private const int MAX_HOURS_PER_DAY = 6; // Maximum teaching hours per day for a programme

    public double EvaluatePenalty(Chromosome chromosome)
    {
        double penalty = 0;
        var programmeGroups = chromosome.Genes
            .SelectMany(g => g.Course.ProgrammeYears
                .Select(py => (ProgrammeYear: py, TimeSlot: g.TimeSlot)))
            .GroupBy(x => x.ProgrammeYear);

        foreach (var group in programmeGroups)
        {
            var dailyHours = group
                .GroupBy(x => x.TimeSlot.Day)
                .Select(g => g.Sum(x => (x.TimeSlot.EndTime - x.TimeSlot.StartTime).TotalHours));

            foreach (var hours in dailyHours)
            {
                if (hours > MAX_HOURS_PER_DAY)
                {
                    penalty += (hours - MAX_HOURS_PER_DAY) * 0.2;
                }
            }
        }
        
        return penalty * PENALTY;
    }

    public string GetViolationMessage(Chromosome chromosome)
    {
        var violations = new List<string>();
        var programmeGroups = chromosome.Genes
            .SelectMany(g => g.Course.ProgrammeYears
                .Select(py => (ProgrammeYear: py, Course: g.Course, TimeSlot: g.TimeSlot)))
            .GroupBy(x => x.ProgrammeYear);

        foreach (var group in programmeGroups)
        {
            var heavyDays = group
                .GroupBy(x => x.TimeSlot.Day)
                .Select(g => (
                    Day: g.Key,
                    Hours: g.Sum(x => (x.TimeSlot.EndTime - x.TimeSlot.StartTime).TotalHours)
                ))
                .Where(x => x.Hours > MAX_HOURS_PER_DAY);

            foreach (var day in heavyDays)
            {
                violations.Add($"{group.Key}: {day.Day} has {day.Hours:F1} hours (max: {MAX_HOURS_PER_DAY})");
            }
        }

        return $"Daily teaching load violations: {string.Join(", ", violations)}";
    }
}

public class LunchBreakConstraint : IConstraint
{
    public const double PENALTY = 0.05;
    private static readonly TimeSpan DEFAULT_DAY_START = new(7, 0, 0);
    private static readonly TimeSpan DEFAULT_DAY_END = new(19, 30, 0);
    private static readonly TimeSpan EXTENDED_DAY_START = new(6, 30, 0);
    private static readonly TimeSpan EXTENDED_DAY_END = new(20, 0, 0);
    private static readonly TimeSpan LUNCH_START = new(12, 0, 0);
    private static readonly TimeSpan LUNCH_END = new(12, 30, 0);

    public double EvaluatePenalty(Chromosome chromosome)
    {
        double penalty = 0;
        var dailySchedules = chromosome.Genes
            .GroupBy(g => g.TimeSlot.Day);

        foreach (var daySchedule in dailySchedules)
        {
            var totalScheduledHours = daySchedule
                .Sum(g => (g.TimeSlot.EndTime - g.TimeSlot.StartTime).TotalHours);

            var timeSlots = daySchedule
                .OrderBy(g => g.TimeSlot.StartTime)
                .ToList();

            var gaps = CalculateTimeGaps(timeSlots, DEFAULT_DAY_START, DEFAULT_DAY_END);
            var hasLunchBreakPossibility = gaps.Any(gap =>
                gap.duration.TotalMinutes >= 30 &&
                gap.start <= LUNCH_START &&
                gap.end >= LUNCH_END);

            if (totalScheduledHours < 12 && !hasLunchBreakPossibility)
            {
                var programmeGroups = daySchedule
                    .SelectMany(g => g.Course.ProgrammeYears
                        .Select(py => (ProgrammeYear: py, TimeSlot: g.TimeSlot)))
                    .GroupBy(x => x.ProgrammeYear);

                foreach (var programme in programmeGroups)
                {
                    var hasLunchTimeClasses = programme
                        .Any(x => x.TimeSlot.StartTime < LUNCH_END &&
                                x.TimeSlot.EndTime > LUNCH_START);

                    if (hasLunchTimeClasses)
                    {
                        penalty += 0.5;
                    }
                }
            }
        }

        return penalty * PENALTY;
    }

    private List<(TimeSpan start, TimeSpan end, TimeSpan duration)> CalculateTimeGaps(
        List<Gene> sortedGenes, TimeSpan dayStart, TimeSpan dayEnd)
    {
        var gaps = new List<(TimeSpan start, TimeSpan end, TimeSpan duration)>();
        var currentTime = dayStart;

        foreach (var gene in sortedGenes)
        {
            if (gene.TimeSlot.StartTime > currentTime)
            {
                gaps.Add((
                    currentTime,
                    gene.TimeSlot.StartTime,
                    gene.TimeSlot.StartTime - currentTime
                ));
            }
            currentTime = gene.TimeSlot.EndTime;
        }

        if (currentTime < dayEnd)
        {
            gaps.Add((currentTime, dayEnd, dayEnd - currentTime));
        }

        return gaps;
    }

    public string GetViolationMessage(Chromosome chromosome)
    {
        var violations = new List<string>();
        var dailySchedules = chromosome.Genes
            .GroupBy(g => g.TimeSlot.Day);

        foreach (var day in dailySchedules)
        {
            var timeSlots = day.OrderBy(g => g.TimeSlot.StartTime).ToList();
            var gaps = CalculateTimeGaps(timeSlots, DEFAULT_DAY_START, DEFAULT_DAY_END);
            var hasLunchBreakPossibility = gaps.Any(gap =>
                gap.duration.TotalMinutes >= 30 &&
                gap.start <= LUNCH_START &&
                gap.end >= LUNCH_END);

            if (!hasLunchBreakPossibility)
            {
                var affectedProgrammes = day
                    .SelectMany(g => g.Course.ProgrammeYears)
                    .Distinct()
                    .Where(py => day
                        .Where(g => g.Course.ProgrammeYears.Contains(py))
                        .Any(g => g.TimeSlot.StartTime < LUNCH_END &&
                                g.TimeSlot.EndTime > LUNCH_START));

                if (affectedProgrammes.Any())
                {
                    violations.Add(
                        $"{day.Key}: No lunch break possible for " +
                        string.Join(", ", affectedProgrammes));
                }
            }
        }

        return violations.Any()
            ? $"Lunch break violations: {string.Join("; ", violations)}"
            : "No lunch break violations";
    }
}

public class CombinedClassConstraint : IConstraint
{
    public const double PENALTY = 0.8; // High penalty as this is important

    public double EvaluatePenalty(Chromosome chromosome)
    {
        double penalty = 0;
        var combinedClasses = chromosome.Genes
            .Where(g => g.Course.ProgrammeYears.Count > 1)
            .GroupBy(g => g.Course.Code);

        foreach (var courseGroup in combinedClasses)
        {
            var course = courseGroup.First().Course;
            var totalStudents = course.StudentCount;
            var programmes = course.ProgrammeYears;
            var departments = programmes.Select(p => p.Code.Substring(0, 2)).Distinct();
            var room = courseGroup.First().Room;

            // Rule 1: Multiple departments should default to virtual
            if (departments.Count() > 1 && course.Mode != CourseMode.Virtual)
            {
                if (room != null) // If room is assigned when it should be virtual
                {
                    penalty += 1.0;
                }
            }

            // Rule 2: Check if hybrid mode is possible and correctly assigned
            if (departments.Count() > 1 && room != null)
            {
                if (room.MaxCapacity < totalStudents)
                {
                    // Should be virtual if no room can accommodate
                    penalty += 1.0;
                }
                else if (course.Mode != CourseMode.Hybrid)
                {
                    // Should be hybrid if room can accommodate
                    penalty += 0.5;
                }
            }

            // Rule 3: Same department combined classes
            if (departments.Count() == 1 && totalStudents > 0)
            {
                if (room == null && course.Mode != CourseMode.Virtual)
                {
                    // Need a room for same department classes unless virtual
                    penalty += 1.0;
                }
                else if (room != null && room.MaxCapacity < totalStudents)
                {
                    // Room too small for combined class
                    penalty += 1.0;
                }
            }
        }

        return penalty * PENALTY;
    }

    public string GetViolationMessage(Chromosome chromosome)
    {
        var violations = new List<string>();
        var combinedClasses = chromosome.Genes
            .Where(g => g.Course.ProgrammeYears.Count > 1)
            .GroupBy(g => g.Course.Code);

        foreach (var courseGroup in combinedClasses)
        {
            var course = courseGroup.First().Course;
            var totalStudents = course.StudentCount;
            var programmes = course.ProgrammeYears;
            var departments = programmes.Select(p => p.Code.Substring(0, 2)).Distinct();
            var room = courseGroup.First().Room;

            var deptList = string.Join(", ", departments);

            if (departments.Count() > 1 && course.Mode != CourseMode.Virtual && room != null)
            {
                violations.Add($"{course.Code}: Multi-department class ({deptList}) should be virtual");
            }

            if (departments.Count() > 1 && room != null && room.MaxCapacity < totalStudents)
            {
                violations.Add($"{course.Code}: Room {room.Name} too small for combined class ({totalStudents} students)");
            }

            if (departments.Count() == 1 && room != null && room.MaxCapacity < totalStudents)
            {
                violations.Add($"{course.Code}: Department {deptList} combined class needs larger room");
            }
        }

        return violations.Any() 
            ? $"Combined class violations: {string.Join(", ", violations)}"
            : "No combined class violations";
    }
}

public class FlexibleScheduleConstraint : IConstraint
{
    public const double PENALTY = 0.1;
    private static readonly TimeSpan EARLIEST_START = new(6, 30, 0);
    private static readonly TimeSpan LATEST_END = new(20, 0, 0);

    public double EvaluatePenalty(Chromosome chromosome)
    {
        double penalty = 0;
        var timeViolations = chromosome.Genes
            .Where(g => g.TimeSlot.StartTime < EARLIEST_START || g.TimeSlot.EndTime > LATEST_END)
            .ToList();

        penalty += timeViolations.Count;

        return penalty * PENALTY;
    }

    public string GetViolationMessage(Chromosome chromosome)
    {
        var violations = chromosome.Genes
            .Where(g => g.TimeSlot.StartTime < EARLIEST_START || g.TimeSlot.EndTime > LATEST_END)
            .Select(g => $"{g.Course.Code} at {g.TimeSlot.StartTime}-{g.TimeSlot.EndTime}")
            .ToList();

        return violations.Any()
            ? $"Flexible schedule violations: {string.Join(", ", violations)}"
            : "No flexible schedule violations";
    }
}

public class RoomUtilizationConstraint : IConstraint
{
    private const double OPTIMAL_UTILIZATION_MIN = 0.7;
    private const double OPTIMAL_UTILIZATION_MAX = 0.9;

    public double EvaluatePenalty(Chromosome chromosome)
    {
        double totalPenalty = 0;
        foreach (var gene in chromosome.Genes)
        {
            if (gene.Room != null && gene.Course.Mode != CourseMode.Virtual)
            {
                double utilization = (double)gene.Course.StudentCount / gene.Room.MaxCapacity;
                
                if (utilization < OPTIMAL_UTILIZATION_MIN)
                {
                    totalPenalty += (OPTIMAL_UTILIZATION_MIN - utilization) * 0.5;
                }
                else if (utilization > OPTIMAL_UTILIZATION_MAX)
                {
                    totalPenalty += (utilization - OPTIMAL_UTILIZATION_MAX) * 0.5;
                }
            }
        }
        return totalPenalty / chromosome.Genes.Count;
    }

    public string GetViolationMessage(Chromosome chromosome)
    {
        var violations = chromosome.Genes
            .Where(g => g.Room != null && g.Course.Mode != CourseMode.Virtual)
            .Where(g => {
                double utilization = (double)g.Course.StudentCount / g.Room.MaxCapacity;
                return utilization < OPTIMAL_UTILIZATION_MIN || utilization > OPTIMAL_UTILIZATION_MAX;
            })
            .Select(g => $"{g.Course.Code} in {g.Room.Name} ({g.Course.StudentCount}/{g.Room.MaxCapacity} = {(double)g.Course.StudentCount / g.Room.MaxCapacity:P0})");

        return $"Room utilization outside optimal range: {string.Join(", ", violations)}";
    }
}

public class PreferredTimeSlotConstraint : IConstraint
{
    public double EvaluatePenalty(Chromosome chromosome)
    {
        double totalPenalty = 0;
        foreach (var gene in chromosome.Genes.Where(g => g.Course?.Preferences != null))
        {
            if (gene.Course.Preferences.PreferredTimeSlots.Any() && 
                !gene.Course.Preferences.PreferredTimeSlots.ContainsKey(gene.TimeSlot))
            {
                totalPenalty += 0.3;
            }
        }
        return totalPenalty / chromosome.Genes.Count;
    }

    public string GetViolationMessage(Chromosome chromosome)
    {
        var violations = chromosome.Genes
            .Where(g => g.Course.Preferences.PreferredTimeSlots.Any() && 
                       !g.Course.Preferences.PreferredTimeSlots.ContainsKey(g.TimeSlot))
            .Select(g => $"{g.Course.Code} scheduled at {g.TimeSlot} instead of preferred slots");

        return $"Courses not in preferred time slots: {string.Join(", ", violations)}";
    }
}

public class ConsecutiveSessionsConstraint : IConstraint
{
    public double EvaluatePenalty(Chromosome chromosome)
    {
        var penalty = 0.0;
        var courseGroups = chromosome.Genes
            .GroupBy(g => g.Course.Code);

        foreach (var group in courseGroups)
        {
            var sessions = group.OrderBy(g => g.TimeSlot.Day)
                              .ThenBy(g => g.TimeSlot.StartTime)
                              .ToList();

            for (int i = 1; i < sessions.Count; i++)
            {
                if (sessions[i].TimeSlot.Day == sessions[i-1].TimeSlot.Day &&
                    !AreSessionsConsecutive(sessions[i-1].TimeSlot, sessions[i].TimeSlot))
                {
                    penalty += 0.2;
                }
            }
        }

        return penalty / chromosome.Genes.Count;
    }

    private bool AreSessionsConsecutive(TimeSlot first, TimeSlot second)
    {
        return first.EndTime == second.StartTime;
    }

    public string GetViolationMessage(Chromosome chromosome)
    {
        var violations = new List<string>();
        var courseGroups = chromosome.Genes
            .GroupBy(g => g.Course.Code);

        foreach (var group in courseGroups)
        {
            var sessions = group.OrderBy(g => g.TimeSlot.Day)
                              .ThenBy(g => g.TimeSlot.StartTime)
                              .ToList();

            for (int i = 1; i < sessions.Count; i++)
            {
                if (sessions[i].TimeSlot.Day == sessions[i-1].TimeSlot.Day &&
                    !AreSessionsConsecutive(sessions[i-1].TimeSlot, sessions[i].TimeSlot))
                {
                    violations.Add($"{group.Key} has non-consecutive sessions on {sessions[i].TimeSlot.Day}");
                }
            }
        }

        return string.Join(", ", violations);
    }
}