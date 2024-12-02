using UMAT_GEN_TTS.Core.Interfaces;
using UMAT_GEN_TTS.Core.Constraints;

namespace UMAT_GEN_TTS.Core.GeneticAlgorithms;

public class BasicFitnessCalculator : IFitnessCalculator
{
    private readonly ConstraintManager _constraintManager;

    public BasicFitnessCalculator()
    {
        _constraintManager = new ConstraintManager();
    }

    public double Calculate(Chromosome chromosome)
    {
        return _constraintManager.EvaluateFitness(chromosome);
    }
} 