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
        foreach (var gene in chromosome.Genes)
        {
            if (gene.Room != null && gene.Course.Mode != CourseMode.Virtual)
            {
                double utilization = (double)gene.Course.StudentCount / gene.Room.Capacity;
                
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
            .Where(g => g.Room != null && g.Course.Mode != CourseMode.Virtual)
            .Where(g => {
                double utilization = (double)g.Course.StudentCount / g.Room.Capacity;
                return utilization < OPTIMAL_UTILIZATION_MIN || utilization > OPTIMAL_UTILIZATION_MAX;
            })
            .Select(g => $"{g.Course.Code} in {g.Room.Name} ({g.Course.StudentCount}/{g.Room.Capacity} = {(double)g.Course.StudentCount / g.Room.Capacity:P0})");
        
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
    public const double PENALTY = 0.1;

    public double EvaluatePenalty(Chromosome chromosome)
    {
        var penalty = 0.0;
        var programmeYearGroups = chromosome.Genes
            .SelectMany(g => g.Course.ProgrammeYears
                .Select(py => (ProgrammeYear: py, TimeSlot: g.TimeSlot)))
            .GroupBy(x => x.ProgrammeYear);

        foreach (var group in programmeYearGroups)
        {
            // Penalize if more than 2 courses on the same day
            var coursesPerDay = group
                .GroupBy(x => x.TimeSlot.Day)
                .Where(g => g.Count() > 2);

            penalty += coursesPerDay.Count() * 0.5;
        }

        return penalty * PENALTY;
    }

    public string GetViolationMessage(Chromosome chromosome)
    {
        var violations = new List<string>();
        var programmeYearGroups = chromosome.Genes
            .SelectMany(g => g.Course.ProgrammeYears
                .Select(py => (ProgrammeYear: py, Course: g.Course, TimeSlot: g.TimeSlot)))
            .GroupBy(x => x.ProgrammeYear);

        foreach (var group in programmeYearGroups)
        {
            var heavyDays = group
                .GroupBy(x => x.TimeSlot.Day)
                .Where(g => g.Count() > 2);

            foreach (var day in heavyDays)
            {
                violations.Add($"{group.Key}: {day.Key} has {day.Count()} courses");
            }
        }

        return $"Programme year spread violations: {string.Join(", ", violations)}";
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
            // For courses that might benefit from lab but don't require it
            if (!gene.Course.RequiresLab && 
                gene.Course.Mode != CourseMode.Virtual &&
                IsTechnicalCourse(gene.Course) &&
                (gene.Room == null || !gene.Room.IsLab))
            {
                penalty += 1.0;
            }
        }
        return penalty * PENALTY;
    }

    private bool IsTechnicalCourse(Course course)
    {
        // Consider courses from technical departments that might benefit from lab access
        var technicalDepartments = new[] 
        { 
            "Computer Science", 
            "Engineering",
            "Physics",
            "Chemistry",
            "Biology"
        };
        
        return technicalDepartments.Contains(course.Department);
    }

    public string GetViolationMessage(Chromosome chromosome)
    {
        var violations = chromosome.Genes
            .Where(g => !g.Course.RequiresLab && 
                       g.Course.Mode != CourseMode.Virtual &&
                       IsTechnicalCourse(g.Course) &&
                       (g.Room == null || !g.Room.IsLab))
            .Select(g => $"{g.Course.Code} ({g.Course.Department})");

        return $"Lab preference not met for technical courses: {string.Join(", ", violations)}";
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
            var departments = programmes.Select(p => p.ProgrammeCode.Substring(0, 2)).Distinct();
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
                if (room.Capacity < totalStudents)
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
                else if (room != null && room.Capacity < totalStudents)
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
            var departments = programmes.Select(p => p.ProgrammeCode.Substring(0, 2)).Distinct();
            var room = courseGroup.First().Room;

            var deptList = string.Join(", ", departments);

            if (departments.Count() > 1 && course.Mode != CourseMode.Virtual && room != null)
            {
                violations.Add($"{course.Code}: Multi-department class ({deptList}) should be virtual");
            }

            if (departments.Count() > 1 && room != null && room.Capacity < totalStudents)
            {
                violations.Add($"{course.Code}: Room {room.Name} too small for combined class ({totalStudents} students)");
            }

            if (departments.Count() == 1 && room != null && room.Capacity < totalStudents)
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