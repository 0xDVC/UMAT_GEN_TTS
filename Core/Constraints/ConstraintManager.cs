using UMAT_GEN_TTS.Core.Interfaces;
using UMAT_GEN_TTS.Core.GeneticAlgorithms;

namespace UMAT_GEN_TTS.Core.Constraints
{
    public class ConstraintManager
    {
        private readonly List<IConstraint> _hardConstraints;
        private readonly List<IConstraint> _softConstraints;

        public ConstraintManager()
        {
            _hardConstraints = new List<IConstraint>
            {
                new TimeSlotConflictConstraint(),
                new RoomConflictConstraint(),
                new RoomCapacityConstraint(),
                new LabRequirementConstraint(),
                new CourseModeSuitabilityConstraint()
            };

            _softConstraints = new List<IConstraint>
            {
                new RoomEfficiencyConstraint(),
                new TimePreferenceConstraint(),
                new ProgrammeYearSpreadConstraint(),
                new LabPreferenceConstraint()
            };
        }

        public double EvaluateFitness(Chromosome chromosome)
        {
            double fitness = 1.0;
            var violations = new List<(string Message, double Penalty)>();

            // Evaluate hard constraints first
            foreach (var constraint in _hardConstraints)
            {
                var penalty = constraint.EvaluatePenalty(chromosome);
                if (penalty > 0)
                {
                    violations.Add((constraint.GetViolationMessage(chromosome), penalty));
                    fitness -= penalty;
                }
            }

            // If hard constraints are severely violated, return minimal fitness
            if (fitness < 0.2) return 0.1;

            // Evaluate soft constraints
            foreach (var constraint in _softConstraints)
            {
                var penalty = constraint.EvaluatePenalty(chromosome);
                if (penalty > 0)
                {
                    violations.Add((constraint.GetViolationMessage(chromosome), penalty));
                    fitness -= penalty * 0.5; // Soft constraints have less impact
                }
            }

            // Log violations if any
            if (violations.Any())
            {
                Console.WriteLine("\nConstraint Violations:");
                foreach (var (message, penalty) in violations.OrderByDescending(v => v.Penalty))
                {
                    Console.WriteLine($"- {message} (Penalty: {penalty:F3})");
                }
            }

            // Ensure fitness stays within valid range
            return Math.Max(0.1, Math.Min(1.0, fitness));
        }
    }
} 