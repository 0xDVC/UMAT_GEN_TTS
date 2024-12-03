using UMAT_GEN_TTS.Core.Interfaces;
using UMAT_GEN_TTS.Core.GeneticAlgorithms;

public class DummyFitnessCalculator : IFitnessCalculator
{
    private readonly double _fitnessValue;

    public DummyFitnessCalculator(double fitnessValue)
    {
        _fitnessValue = fitnessValue;
    }

    public double CalculateFitness(Chromosome chromosome)
    {
        return _fitnessValue; // Return the dummy fitness value
    }
} 