using UMAT_GEN_TTS.Core.Interfaces;
using UMAT_GEN_TTS.Core.GeneticAlgorithms;
using UMAT_GEN_TTS.Core.Models;

namespace UMAT_GEN_TTS.Core.Constraints;

public class TimeSlotConflictConstraint : IConstraint
{
    public const double PENALTY = 0.2;

    public double EvaluatePenalty(Chromosome chromosome)
    {
        var conflicts = 0;
        var genes = chromosome.Genes;

        for (int i = 0; i < genes.Count; i++)
        {
            for (int j = i + 1; j < genes.Count; j++)
            {
                if (genes[i].TimeSlot.Overlaps(genes[j].TimeSlot))
                {
                    if (SharesProgrammeYears(genes[i].Course, genes[j].Course))
                    {
                        conflicts++;
                    }
                }
            }
        }

        return conflicts * PENALTY;
    }

    private bool SharesProgrammeYears(Course course1, Course course2)
    {
        if (course1?.ProgrammeYears == null || course2?.ProgrammeYears == null)
            return false;

        return course1.ProgrammeYears.Any(py1 => 
            course2.ProgrammeYears.Any(py2 => 
                py1?.Code == py2?.Code && py1?.Year == py2?.Year));
    }

    public string GetViolationMessage(Chromosome chromosome)
    {
        var conflicts = 0;
        var genes = chromosome.Genes;

        for (int i = 0; i < genes.Count; i++)
        {
            for (int j = i + 1; j < genes.Count; j++)
            {
                if (genes[i].TimeSlot.Overlaps(genes[j].TimeSlot))
                {
                    if (SharesProgrammeYears(genes[i].Course, genes[j].Course))
                    {
                        conflicts++;
                    }
                }
            }
        }
        return $"Time slot conflicts found: {conflicts} violations";
    }
}

public class RoomConflictConstraint : IConstraint
{
    public const double PENALTY = 0.2;

    public double EvaluatePenalty(Chromosome chromosome)
    {
        var conflicts = 0;
        var genes = chromosome.Genes;

        for (int i = 0; i < genes.Count; i++)
        {
            for (int j = i + 1; j < genes.Count; j++)
            {
                if (genes[i].TimeSlot.Overlaps(genes[j].TimeSlot) && 
                    genes[i].Room == genes[j].Room)
                {
                    conflicts++;
                }
            }
        }
        return conflicts * PENALTY;
    }

    public string GetViolationMessage(Chromosome chromosome)
    {
        var conflicts = 0;
        var genes = chromosome.Genes;

        for (int i = 0; i < genes.Count; i++)
        {
            for (int j = i + 1; j < genes.Count; j++)
            {
                if (genes[i].TimeSlot.Overlaps(genes[j].TimeSlot) && 
                    genes[i].Room == genes[j].Room)
                {
                    conflicts++;
                }
            }
        }
        return $"Room conflicts found: {conflicts} violations";
    }
}

public class RoomCapacityConstraint : IConstraint
{
    public const double PENALTY = 0.3;

    public double EvaluatePenalty(Chromosome chromosome)
    {
        if (chromosome?.Genes == null) return 0;
        
        double penalty = 0;
        foreach (var gene in chromosome.Genes)
        {
            if (gene?.Course == null) continue;
            if (gene.Room != null && gene.Room.MaxCapacity < gene.Course.StudentCount)
            {
                penalty += 1.0 * (1 - (double)gene.Room.MaxCapacity / gene.Course.StudentCount);
            }
        }
        return penalty * PENALTY;
    }

    public string GetViolationMessage(Chromosome chromosome)
    {
        var violations = chromosome.Genes
            .Where(g => g.Room != null && g.Room.MaxCapacity < g.Course.StudentCount)
            .Select(g => $"{g.Course.Code} requires capacity for {g.Course.StudentCount} but {g.Room?.Name} only fits {g.Room?.MaxCapacity}");
        return $"Room capacity violations: {string.Join(", ", violations)}";
    }
}

public class LabRequirementConstraint : IConstraint
{
    public const double PENALTY = 0.25;

    public double EvaluatePenalty(Chromosome chromosome)
    {
        double penalty = 0;
        foreach (var gene in chromosome.Genes)
        {
            if (gene.Course.RequiresLab && 
                gene.Course.Mode != CourseMode.Virtual &&
                (gene.Room == null || !gene.Room.IsLab))
            {
                penalty += 1.0;
            }
        }
        return penalty * PENALTY;
    }

    public string GetViolationMessage(Chromosome chromosome)
    {
        var violations = chromosome.Genes
            .Where(g => g.Course.RequiresLab && 
                       g.Course.Mode != CourseMode.Virtual &&
                       (g.Room == null || !g.Room.IsLab))
            .Select(g => $"{g.Course.Code} requires lab but assigned to {g.Room?.Name ?? "no room"}");

        return $"Lab requirement violations: {string.Join(", ", violations)}";
    }
}

public class CourseModeSuitabilityConstraint : IConstraint
{
    public const double PENALTY = 0.2;

    public double EvaluatePenalty(Chromosome chromosome)
    {
        double penalty = 0;
        foreach (var gene in chromosome.Genes)
        {
            switch (gene.Course.Mode)
            {
                case CourseMode.Virtual when gene.Room != null:
                    penalty += 1.0; // Virtual courses shouldn't have rooms
                    break;
                case CourseMode.Regular when gene.Room == null:
                    penalty += 1.0; // Regular courses must have rooms
                    break;
            }
        }
        return penalty * PENALTY;
    }

    public string GetViolationMessage(Chromosome chromosome)
    {
        var virtualWithRoom = chromosome.Genes
            .Where(g => g.Course.Mode == CourseMode.Virtual && g.Room != null)
            .Select(g => $"{g.Course.Code} (Virtual with room)");
        
        var physicalNoRoom = chromosome.Genes
            .Where(g => g.Course.Mode == CourseMode.Regular && g.Room == null)
            .Select(g => $"{g.Course.Code} (Regular without room)");

        return $"Course mode violations: {string.Join(", ", virtualWithRoom.Concat(physicalNoRoom))}";
    }
}

public class LecturerConflictConstraint : IConstraint
{
    public const double PENALTY = 0.2;

    public double EvaluatePenalty(Chromosome chromosome)
    {
        return chromosome.Genes
            .Where(g => g.Course?.Lecturer != null)
            .Count(g => 
                chromosome.Genes.Any(other => 
                    other != g && 
                    other.Course?.Lecturer == g.Course.Lecturer && 
                    other.TimeSlot.Overlaps(g.TimeSlot)));
    }

    public string GetViolationMessage(Chromosome chromosome)
    {
        return string.Join(", ", chromosome.Genes
            .Where(g => g.Course?.Lecturer != null)
            .Where(g => chromosome.Genes.Any(other => 
                other != g && 
                other.Course?.Lecturer == g.Course.Lecturer && 
                other.TimeSlot.Overlaps(g.TimeSlot)))
            .Select(g => $"{g.Course.Code} has lecturer conflict"));
    }
}

public class DepartmentRoomConstraint : IConstraint
{
    public const double PENALTY = 0.3;

    public double EvaluatePenalty(Chromosome chromosome)
    {
        double penalty = 0;
        foreach (var gene in chromosome.Genes)
        {
            if (gene.Room != null && 
                gene.Course.Mode != CourseMode.Virtual &&
                gene.Room.Ownership == RoomOwnership.Departmental &&
                gene.Room.AssignedDepartment != gene.Course.AssignedDepartment)
            {
                penalty += 1.0;
            }
        }
        return penalty * PENALTY;
    }

    public string GetViolationMessage(Chromosome chromosome)
    {
        var violations = chromosome.Genes
            .Where(g => g.Room != null && 
                   g.Course.Mode != CourseMode.Virtual &&
                   g.Room.Ownership == RoomOwnership.Departmental &&
                   g.Room.AssignedDepartment != g.Course.AssignedDepartment)
            .Select(g => $"{g.Course.Code} using room {g.Room?.Name} from different department");

        return $"Department room violations: {string.Join(", ", violations)}";
    }
}

public class LecturerAvailabilityConstraint : IConstraint
{
    public const double PENALTY = 0.4;

    public double EvaluatePenalty(Chromosome chromosome)
    {
        if (chromosome?.Genes == null) return 0;

        double penalty = 0;
        foreach (var gene in chromosome.Genes)
        {
            if (gene?.Course?.Lecturer == null || gene.TimeSlot == null) continue;

            // Check if scheduled on days lecturer is not available
            if (gene.Course.Lecturer.DaysNotAvailable?.Contains(gene.TimeSlot.Day) == true)
            {
                penalty += 1.0;
            }

            // Check if exceeds daily hours limit
            var dailyHours = chromosome.Genes
                .Where(g => g?.Course?.Lecturer == gene.Course.Lecturer &&
                           g?.TimeSlot?.Day == gene.TimeSlot.Day)
                .Sum(g => (g.TimeSlot.EndTime - g.TimeSlot.StartTime).TotalHours);

            if (dailyHours > gene.Course.Lecturer.MaxDailyHours)
            {
                penalty += (dailyHours - gene.Course.Lecturer.MaxDailyHours) / gene.Course.Lecturer.MaxDailyHours;
            }
        }
        return penalty * PENALTY;
    }

    public string GetViolationMessage(Chromosome chromosome)
    {
        if (chromosome?.Genes == null) return "No genes to evaluate";
        var violations = new List<string>();
        
        var lecturerGroups = chromosome.Genes
            .Where(g => g?.Course?.Lecturer != null)
            .GroupBy(g => g.Course.Lecturer);

        foreach (var group in lecturerGroups)
        {
            var lecturer = group.Key;
            if (lecturer?.DaysNotAvailable == null || lecturer.Name == null) continue;

            var unavailableDayViolations = group
                .Where(g => g?.TimeSlot != null && lecturer.DaysNotAvailable.Contains(g.TimeSlot.Day))
                .Select(g => $"{lecturer.Name}: {g.Course.Code} scheduled on unavailable day {g.TimeSlot.Day}");
            violations.AddRange(unavailableDayViolations);

            var dailyOverloads = group
                .Where(g => g?.TimeSlot != null)
                .GroupBy(g => g.TimeSlot.Day)
                .Where(d => d.Sum(g => (g.TimeSlot.EndTime - g.TimeSlot.StartTime).TotalHours) > lecturer.MaxDailyHours)
                .Select(d => $"{lecturer.Name}: Exceeds daily limit on {d.Key}");
            violations.AddRange(dailyOverloads);
        }

        return $"Lecturer availability violations: {string.Join(", ", violations)}";
    }
}

public class ProgrammeYearConstraint : IConstraint
{
    public const double PENALTY = 0.35;

    public double EvaluatePenalty(Chromosome chromosome)
    {
        if (chromosome?.Genes == null) return 0;

        double penalty = 0;
        
        // Group courses by programme
        var programmeGroups = chromosome.Genes
            .Where(g => g?.Course?.ProgrammeYears != null)
            .SelectMany(g => g.Course.ProgrammeYears
                .Where(py => py != null)
                .Select(py => new { Gene = g, Programme = py }))
            .GroupBy(x => x.Programme.Code);

        foreach (var group in programmeGroups)
        {
            // Calculate total weekly hours instead of credit hours
            var totalWeeklyHours = group
                .Sum(x => x.Gene.Course.WeeklyHours);

            var maxWeeklyHours = group.First().Programme.MaxWeeklyHours;
            
            if (maxWeeklyHours > 0 && totalWeeklyHours > maxWeeklyHours)
            {
                penalty += (totalWeeklyHours - maxWeeklyHours) / (double)maxWeeklyHours;
            }
        }

        return penalty * PENALTY;
    }

    public string GetViolationMessage(Chromosome chromosome)
    {
        if (chromosome?.Genes == null) return "No genes to evaluate";
        var violations = new List<string>();

        var programmeGroups = chromosome.Genes
            .Where(g => g?.Course?.ProgrammeYears != null)
            .SelectMany(g => g.Course.ProgrammeYears
                .Where(py => py != null)
                .Select(py => new { Gene = g, Programme = py }))
            .GroupBy(x => x.Programme.Code);

        foreach (var group in programmeGroups)
        {
            var totalWeeklyHours = group
                .Sum(x => x.Gene.Course.WeeklyHours);

            var maxWeeklyHours = group.First().Programme.MaxWeeklyHours;

            if (maxWeeklyHours > 0 && totalWeeklyHours > maxWeeklyHours)
            {
                violations.Add(
                    $"{group.Key}: Exceeds max weekly hours " +
                    $"({totalWeeklyHours}/{maxWeeklyHours})");
            }
        }

        return $"Programme scheduling violations: {string.Join(", ", violations)}";
    }
}

public class RoomConstraint : IConstraint
{
    public double EvaluatePenalty(Chromosome chromosome) =>
        chromosome.Genes.Count(g => 
            g.Room != null && 
            g.Course.StudentCount > g.Room.MaxCapacity);

    public string GetViolationMessage(Chromosome chromosome) =>
        string.Join(", ", chromosome.Genes
            .Where(g => g.Room != null && g.Course.StudentCount > g.Room.MaxCapacity)
            .Select(g => $"{g.Course.Code} exceeds room capacity ({g.Course.StudentCount}/{g.Room.MaxCapacity})"));
}

public class TimeSlotConstraint : IConstraint
{
    public double EvaluatePenalty(Chromosome chromosome) =>
        chromosome.Genes.Count(g => 
            chromosome.Genes.Any(other => other != g && 
                other.TimeSlot.Overlaps(g.TimeSlot)));

    public string GetViolationMessage(Chromosome chromosome) =>
        string.Join(", ", chromosome.Genes
            .Where(g => chromosome.Genes.Any(other => other != g && 
                other.TimeSlot.Overlaps(g.TimeSlot)))
            .Select(g => $"{g.Course.Code} has time conflict"));
}

public class LecturerConstraint : IConstraint
{
    public double EvaluatePenalty(Chromosome chromosome) =>
        chromosome.Genes.Count(g => 
            chromosome.Genes.Any(other => other != g && 
                other.Course.Lecturer == g.Course.Lecturer && 
                other.TimeSlot.Overlaps(g.TimeSlot)));

    public string GetViolationMessage(Chromosome chromosome) =>
        string.Join(", ", chromosome.Genes
            .Where(g => chromosome.Genes.Any(other => other != g && 
                other.Course.Lecturer == g.Course.Lecturer && 
                other.TimeSlot.Overlaps(g.TimeSlot)))
            .Select(g => $"{g.Course.Code} has lecturer conflict"));
} 

public class SessionDurationConstraint : IConstraint
{
    public const double PENALTY = 0.3;

    public double EvaluatePenalty(Chromosome chromosome)
    {
        double penalty = 0;
        foreach (var gene in chromosome.Genes)
        {
            var sessionDuration = TimeSpan.FromHours(gene.Course.SessionDuration);
            var slotDuration = gene.TimeSlot.EndTime - gene.TimeSlot.StartTime;

            if (sessionDuration > slotDuration)
            {
                penalty += 1.0;
            }
        }
        return penalty * PENALTY;
    }

    public string GetViolationMessage(Chromosome chromosome)
    {
        var violations = chromosome.Genes
            .Where(g => 
            {
                var sessionDuration = TimeSpan.FromHours(g.Course.SessionDuration);
                var slotDuration = g.TimeSlot.EndTime - g.TimeSlot.StartTime;
                return sessionDuration > slotDuration;
            })
            .Select(g => $"{g.Course.Code}: Session duration {g.Course.SessionDuration}h exceeds slot duration");

        return $"Session duration violations: {string.Join(", ", violations)}";
    }
}

public class WeeklyHoursConstraint : IConstraint
{
    public const double PENALTY = 0.4;

    public double EvaluatePenalty(Chromosome chromosome)
    {
        var courseGroups = chromosome.Genes
            .GroupBy(g => g.Course.Code);

        double penalty = 0;
        foreach (var group in courseGroups)
        {
            var scheduledHours = group.Sum(g => g.Course.SessionDuration);
            var requiredHours = group.First().Course.WeeklyHours;

            if (scheduledHours != requiredHours)
            {
                penalty += Math.Abs(scheduledHours - requiredHours) / requiredHours;
            }
        }
        return penalty * PENALTY;
    }

    public string GetViolationMessage(Chromosome chromosome)
    {
        var violations = chromosome.Genes
            .GroupBy(g => g.Course.Code)
            .Where(g => 
            {
                var scheduledHours = g.Sum(x => x.Course.SessionDuration);
                var requiredHours = g.First().Course.WeeklyHours;
                return scheduledHours != requiredHours;
            })
            .Select(g => $"{g.Key}: Scheduled {g.Sum(x => x.Course.SessionDuration)}h vs required {g.First().Course.WeeklyHours}h");

        return $"Weekly hours violations: {string.Join(", ", violations)}";
    }
}