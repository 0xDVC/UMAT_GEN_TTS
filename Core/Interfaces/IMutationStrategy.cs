using UMAT_GEN_TTS.Core.GeneticAlgorithms;

namespace UMAT_GEN_TTS.Core.Interfaces;

public interface IMutationStrategy
{
    void Mutate(Chromosome chromosome);
    double BaseMutationRate { get; }
    void SetCurrentRate(double rate);
} 