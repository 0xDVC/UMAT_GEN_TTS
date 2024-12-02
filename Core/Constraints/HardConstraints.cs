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
        return course1.ProgrammeYears.Any(py1 => 
            course2.ProgrammeYears.Any(py2 => 
                py1.ProgrammeCode == py2.ProgrammeCode && py1.Year == py2.Year));
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
        double penalty = 0;
        foreach (var gene in chromosome.Genes)
        {
            if (gene.Room != null && gene.Room.Capacity < gene.Course.StudentCount)
            {
                penalty += 1.0 * (1 - (double)gene.Room.Capacity / gene.Course.StudentCount);
            }
        }
        return penalty * PENALTY;
    }

    public string GetViolationMessage(Chromosome chromosome)
    {
        var violations = chromosome.Genes
            .Where(g => g.Room != null && g.Room.Capacity < g.Course.StudentCount)
            .Select(g => $"{g.Course.Code} requires capacity for {g.Course.StudentCount} but {g.Room.Name} only fits {g.Room.Capacity}");
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
                case CourseMode.Physical when gene.Room == null:
                    penalty += 1.0; // Physical courses must have rooms
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
            .Where(g => g.Course.Mode == CourseMode.Physical && g.Room == null)
            .Select(g => $"{g.Course.Code} (Physical without room)");

        return $"Course mode violations: {string.Join(", ", virtualWithRoom.Concat(physicalNoRoom))}";
    }
} 