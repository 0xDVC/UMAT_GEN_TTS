using UMAT_GEN_TTS.Core.Interfaces;

namespace UMAT_GEN_TTS.Core.GeneticAlgorithms;
public class Chromosome
{
    public List<Gene> Genes { get; private set; } = new List<Gene>();

    public Chromosome()
    {
    }

    public Chromosome(List<Gene> genes)
    {
        Genes = genes;
    }

    public double Fitness { get; private set; }

    public void CalculateFitness(IFitnessCalculator calculator)
    {
        Fitness = calculator.CalculateFitness(this);
    }
} 