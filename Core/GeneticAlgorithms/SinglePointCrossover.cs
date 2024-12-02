using UMAT_GEN_TTS.Core.Interfaces;
using UMAT_GEN_TTS.Core.Models;

namespace UMAT_GEN_TTS.Core.GeneticAlgorithms;

public class SinglePointCrossover : ICrossoverStrategy
{
    public (Chromosome, Chromosome) Crossover(Chromosome parent1, Chromosome parent2)
    {
        var crossoverPoint = Random.Shared.Next(1, parent1.Genes.Count - 1);
        
        // Create new gene lists ensuring virtual courses have no rooms
        var child1Genes = parent1.Genes.Take(crossoverPoint)
            .Concat(parent2.Genes.Skip(crossoverPoint))
            .Select(g => new Gene(
                g.Course,
                g.TimeSlot,
                g.Course.Mode == CourseMode.Virtual ? null : g.Room,
                g.IsCompromised))
            .ToList();

        var child2Genes = parent2.Genes.Take(crossoverPoint)
            .Concat(parent1.Genes.Skip(crossoverPoint))
            .Select(g => new Gene(
                g.Course,
                g.TimeSlot,
                g.Course.Mode == CourseMode.Virtual ? null : g.Room,
                g.IsCompromised))
            .ToList();

        return (new Chromosome(child1Genes), new Chromosome(child2Genes));
    }
} 