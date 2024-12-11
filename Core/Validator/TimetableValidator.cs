using UMAT_GEN_TTS.Core.Models;
using UMAT_GEN_TTS.Core.Constraints;
using UMAT_GEN_TTS.Core.GeneticAlgorithms;

namespace UMAT_GEN_TTS.Core.Validator;

public class TimetableValidator
{
    private readonly ConstraintManager _constraintManager;

    public TimetableValidator(ConstraintManager constraintManager)
    {
        _constraintManager = constraintManager;
    }

    public virtual ValidationResult ValidateSolution(Chromosome solution)
    {
        if (solution?.Genes == null)
        {
            return new ValidationResult 
            { 
                IsValid = false,
                Violations = new List<string> { "Invalid solution: null or no genes" }
            };
        }
        // Evaluate using ConstraintManager
        var hardPenalty = _constraintManager.EvaluateHardConstraints(solution);
        var softPenalty = _constraintManager.EvaluateSoftConstraints(solution);
        var violations = _constraintManager.GetViolationMessages(solution);

        var result = new ValidationResult
        {
            IsValid = hardPenalty == 0,
            FinalFitness = solution.Fitness,
            Violations = violations.ToList(),
            Metrics = new()
            {
                ["HardConstraints"] = 1 - hardPenalty,
                ["SoftConstraints"] = 1 - softPenalty,
            }
        };

        CalculateResourceUtilization(solution, result);
        return result;
    }

    private void CalculateResourceUtilization(Chromosome solution, ValidationResult result)
    {
        // Room utilization stats
        var roomStats = solution.Genes
            .Where(g => g.Room != null && g.Course != null)
            .GroupBy(g => g.Room!)
            .ToDictionary(
                g => g.Key.Name ?? "Unknown",
                g => new
                {
                    UsageCount = g.Count(),
                    AverageUtilization = g.Average(x => (double)x.Course.StudentCount / (g.Key.MaxCapacity > 0 ? g.Key.MaxCapacity : 1))
                });

        result.Metrics["AverageRoomUtilization"] = roomStats.Values.Average(s => s.AverageUtilization);
        result.Metrics["RoomUsageVariance"] = roomStats.Values.Average(s => s.UsageCount);

        // Lecturer workload distribution
        var lecturerStats = solution.Genes
            .Where(g => g.Course?.Lecturer != null)
            .GroupBy(g => g.Course.Lecturer)
            .ToDictionary(
                g => g.Key?.Name ?? "Unknown",
                g => g.Sum(x => x.Course.WeeklyHours));

        result.Metrics["AverageLecturerHours"] = lecturerStats.Values.Average();
        result.Metrics["LecturerLoadVariance"] = CalculateStandardDeviation(lecturerStats.Values);
    }

    private double CalculateStandardDeviation(IEnumerable<int> values)
    {
        var avg = values.Average();
        var variance = values.Average(v => Math.Pow(v - avg, 2));
        return Math.Sqrt(variance);
    }
} 