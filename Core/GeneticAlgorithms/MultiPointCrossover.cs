using UMAT_GEN_TTS.Core.Interfaces;
using UMAT_GEN_TTS.Core.Models;

namespace UMAT_GEN_TTS.Core.GeneticAlgorithms;

public class MultiPointCrossover : ICrossoverStrategy
{
    private readonly Random _random = new();
    private readonly int _numPoints;

    public MultiPointCrossover(int numPoints = 2)
    {
        _numPoints = numPoints;
    }

    public (Chromosome, Chromosome) Crossover(Chromosome parent1, Chromosome parent2)
    {
        // Ensure we have valid gene counts
        int minGenes = Math.Min(parent1.Genes.Count, parent2.Genes.Count);
        if (minGenes < 2) return (parent1, parent2); // Can't crossover with less than 2 genes

        // Pick two random points for crossover
        int point1 = Random.Shared.Next(0, minGenes - 1);
        int point2 = Random.Shared.Next(point1 + 1, minGenes);

        if (parent1.Genes.Count != parent2.Genes.Count)
            throw new ArgumentException("Parents must have same number of genes");

        // Generate crossover points
        var points = new List<int>();
        for (int i = 0; i < _numPoints; i++)
        {
            var point = _random.Next(1, parent1.Genes.Count - 1);
            while (points.Contains(point))
                point = _random.Next(1, parent1.Genes.Count - 1);
            points.Add(point);
        }
        points.Sort();

        // Create offspring genes
        var child1Genes = new List<Gene>();
        var child2Genes = new List<Gene>();
        var useParent1 = true;

        for (int i = 0; i < parent1.Genes.Count; i++)
        {
            if (points.Contains(i))
                useParent1 = !useParent1;

            // Create new genes to avoid reference issues
            var gene1 = useParent1 ? parent1.Genes[i] : parent2.Genes[i];
            var gene2 = useParent1 ? parent2.Genes[i] : parent1.Genes[i];

            child1Genes.Add(new Gene(
                gene1.Course,
                gene1.TimeSlot,
                gene1.Course.Mode == CourseMode.Virtual ? null : gene1.Room,
                gene1.IsCompromised));

            child2Genes.Add(new Gene(
                gene2.Course,
                gene2.TimeSlot,
                gene2.Course.Mode == CourseMode.Virtual ? null : gene2.Room,
                gene2.IsCompromised));
        }

        return (new Chromosome(child1Genes), new Chromosome(child2Genes));
    }
} 