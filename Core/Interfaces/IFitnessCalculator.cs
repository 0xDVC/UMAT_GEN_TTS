using UMAT_GEN_TTS.Core.GeneticAlgorithms;

namespace UMAT_GEN_TTS.Core.Interfaces;
public interface IFitnessCalculator
{
    double Calculate(Chromosome chromosome);
} 