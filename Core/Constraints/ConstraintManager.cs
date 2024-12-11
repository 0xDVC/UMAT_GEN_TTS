using UMAT_GEN_TTS.Core.Interfaces;
using UMAT_GEN_TTS.Core.GeneticAlgorithms;

namespace UMAT_GEN_TTS.Core.Constraints
{
    public class ConstraintManager : IFitnessCalculator
    {
        private readonly List<IConstraint> _hardConstraints = new();
        private readonly List<IConstraint> _softConstraints = new();
        private readonly Dictionary<string, double> _hardConstraintWeights = new();
        private readonly Dictionary<string, double> _softConstraintWeights = new();

        private void AddHardConstraint(IConstraint constraint)
        {
            _hardConstraints.Add(constraint);
            _hardConstraintWeights[constraint.GetType().Name] = 1.0; // Default weight
        }

        private void AddSoftConstraint(IConstraint constraint)
        {
            _softConstraints.Add(constraint);
            _softConstraintWeights[constraint.GetType().Name] = 0.5; // Default weight
        }

        public ConstraintManager()
        {
            // Hard Constraints
            AddHardConstraint(new TimeSlotConflictConstraint());
            AddHardConstraint(new RoomConflictConstraint());
            AddHardConstraint(new RoomCapacityConstraint());
            AddHardConstraint(new LabRequirementConstraint());
            AddHardConstraint(new CourseModeSuitabilityConstraint());
            AddHardConstraint(new LecturerConflictConstraint());
            AddHardConstraint(new DepartmentRoomConstraint());
            AddHardConstraint(new LecturerAvailabilityConstraint());
            AddHardConstraint(new SessionDurationConstraint());
            AddHardConstraint(new WeeklyHoursConstraint());

            // Soft Constraints
            AddSoftConstraint(new RoomEfficiencyConstraint());
            AddSoftConstraint(new TimePreferenceConstraint());
            AddSoftConstraint(new ProgrammeYearSpreadConstraint());
            AddSoftConstraint(new LabPreferenceConstraint());
            AddSoftConstraint(new PreferredTimeSlotConstraint());
            AddSoftConstraint(new ConsecutiveSessionsConstraint());
        }

        public double CalculateFitness(Chromosome chromosome)
        {
            double hardConstraintPenalty = EvaluateHardConstraints(chromosome);
            double softConstraintPenalty = EvaluateSoftConstraints(chromosome);
            
            // Calculate base fitness from hard constraints
            double hardFitness = Math.Max(0, 1.0 - (hardConstraintPenalty * 0.2));
            
            // Add bonus from soft constraints
            double softFitness = Math.Max(0, 1.0 - (softConstraintPenalty * 0.1));
            
            // Weighted combination
            double fitness = (hardFitness * 0.8) + (softFitness * 0.2);
            
            return Math.Max(0.1, Math.Min(1, fitness)); // Ensure minimum fitness of 0.1
        }

        public virtual double EvaluateHardConstraints(Chromosome solution) =>
            _hardConstraints.Sum(c => 
                c.EvaluatePenalty(solution) * 
                _hardConstraintWeights[c.GetType().Name]);

        public virtual double EvaluateSoftConstraints(Chromosome solution) =>
            _softConstraints.Sum(c => 
                c.EvaluatePenalty(solution) * 
                _softConstraintWeights[c.GetType().Name]);

        public virtual List<string> GetViolationMessages(Chromosome solution) =>
            _hardConstraints.Concat(_softConstraints)
                .Select(c => c.GetViolationMessage(solution))
                .Where(m => !string.IsNullOrEmpty(m))
                .ToList();

        public void SetHardConstraintWeight(string constraintName, double weight) =>
            _hardConstraintWeights[constraintName] = weight;

        public void SetSoftConstraintWeight(string constraintName, double weight) =>
            _softConstraintWeights[constraintName] = weight;
    }
}