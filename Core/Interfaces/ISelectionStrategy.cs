using UMAT_GEN_TTS.Core.GeneticAlgorithms;

namespace UMAT_GEN_TTS.Core.Interfaces;

public interface ISelectionStrategy
{
    List<Chromosome> Select(List<Chromosome> population, int numberToSelect);
} 