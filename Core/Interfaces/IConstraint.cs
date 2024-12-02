using UMAT_GEN_TTS.Core.GeneticAlgorithms;

namespace UMAT_GEN_TTS.Core.Interfaces;
public interface IConstraint
{
    double EvaluatePenalty(Chromosome chromosome);
    string GetViolationMessage(Chromosome chromosome);
} 