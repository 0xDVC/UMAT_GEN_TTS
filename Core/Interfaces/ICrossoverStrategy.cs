using UMAT_GEN_TTS.Core.GeneticAlgorithms;

namespace UMAT_GEN_TTS.Core.Interfaces;
public interface ICrossoverStrategy
{
    (Chromosome, Chromosome) Crossover(Chromosome parent1, Chromosome parent2);
} 