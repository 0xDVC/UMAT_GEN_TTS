using UMAT_GEN_TTS.Core.Interfaces;
using UMAT_GEN_TTS.Core.Constraints;

namespace UMAT_GEN_TTS.Core.GeneticAlgorithms;

public class FitnessCalculator : IFitnessCalculator
{
    private readonly ConstraintManager _constraintManager;

    public FitnessCalculator()
    {
        _constraintManager = new ConstraintManager();
    }

    public double Calculate(Chromosome chromosome)
    {
        return _constraintManager.EvaluateFitness(chromosome);
    }
} 