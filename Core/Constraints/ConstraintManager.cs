using UMAT_GEN_TTS.Core.Interfaces;
using UMAT_GEN_TTS.Core.GeneticAlgorithms;

namespace UMAT_GEN_TTS.Core.Constraints
{
    public class ConstraintManager : IFitnessCalculator
    {
        private readonly List<(IConstraint Constraint, double Weight)> _constraints;

        public ConstraintManager()
        {
            _constraints = new List<(IConstraint, double)>
            {
                // Hard Constraints (higher weights)
                (new RoomConflictConstraint(), 1.0),
                (new RoomCapacityConstraint(), 1.0),
                (new TimeSlotConflictConstraint(), 1.0),
                (new CourseModeSuitabilityConstraint(), 1.0),
                (new LabRequirementConstraint(), 1.0),
                (new LecturerConflictConstraint(), 1.0),

                // Soft Constraints (lower weights)
                (new RoomEfficiencyConstraint(), 0.5),
                (new TimePreferenceConstraint(), 0.3),
                (new ConsecutiveLecturesConstraint(), 0.3),
                (new DailyTeachingLoadConstraint(), 0.4),
                (new LunchBreakConstraint(), 0.4),
                (new ProgrammeYearSpreadConstraint(), 0.3),
                (new CombinedClassConstraint(), 0.8),
                (new FlexibleScheduleConstraint(), 0.2)
            };
        }

        public double CalculateFitness(Chromosome chromosome)
        {
            double totalPenalty = 0;
            var violations = new List<string>();

            foreach (var (constraint, weight) in _constraints)
            {
                var penalty = constraint.EvaluatePenalty(chromosome) * weight;
                if (penalty > 0)
                {
                    totalPenalty += penalty;
                    violations.Add(constraint.GetViolationMessage(chromosome));
                }
            }

            // Print violations if any exist
            if (violations.Any())
            {
                Console.WriteLine("\nConstraint Violations:");
                foreach (var violation in violations)
                {
                    Console.WriteLine($"- {violation}");
                }
            }

            // Calculate fitness (1 is perfect, 0 is worst)
            return Math.Max(0, 1 - totalPenalty);
        }
    }
} 