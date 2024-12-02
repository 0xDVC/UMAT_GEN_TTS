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