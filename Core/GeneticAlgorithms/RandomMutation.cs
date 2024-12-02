using UMAT_GEN_TTS.Core.Interfaces;
using UMAT_GEN_TTS.Core.Models;

namespace UMAT_GEN_TTS.Core.GeneticAlgorithms;

public class RandomMutation : IMutationStrategy
{
    private readonly Random _random = new();
    private readonly double _mutationRate;

    public RandomMutation(double mutationRate = 0.1)
    {
        _mutationRate = mutationRate;
    }

    public void Mutate(Chromosome chromosome)
    {
        foreach (var gene in chromosome.Genes)
        {
            if (_random.NextDouble() > _mutationRate)
                continue;

            // Don't mutate room for virtual courses
            if (gene.Course.Mode == CourseMode.Virtual)
            {
                gene.Room = null;
                MutateTimeSlot(gene);
                continue;
            }

            // For non-virtual courses, randomly choose what to mutate
            var mutationType = _random.Next(2); // 0: Room, 1: TimeSlot

            switch (mutationType)
            {
                case 0:
                    MutateRoom(gene);
                    break;
                case 1:
                    MutateTimeSlot(gene);
                    break;
            }
        }
    }

    private void MutateRoom(Gene gene)
    {
        // Skip room mutation for virtual courses
        if (gene.Course.Mode == CourseMode.Virtual)
        {
            gene.Room = null;
            return;
        }

        var availableRooms = TestDataGenerator.GenerateSampleRooms(5)
            .Where(r =>
                r.Capacity >= gene.Course.StudentCount &&
                r.IsLab == gene.Course.RequiresLab)
            .ToList();

        if (availableRooms.Any())
        {
            var newRoom = availableRooms[_random.Next(availableRooms.Count)];
            Console.WriteLine($"Mutated room for course {gene.Course.Code}: {gene.Room?.Name ?? "none"} -> {newRoom.Name}");
            gene.Room = newRoom;
        }
    }

    private void MutateTimeSlot(Gene gene)
    {
        var availableTimeSlots = TestDataGenerator.GenerateTimeSlots();
        if (availableTimeSlots.Any())
        {
            var newTimeSlot = availableTimeSlots[_random.Next(availableTimeSlots.Count)];
            Console.WriteLine($"Mutated time slot for course {gene.Course.Code}: " +
                            $"{gene.TimeSlot.Day} {gene.TimeSlot.StartTime} -> " +
                            $"{newTimeSlot.Day} {newTimeSlot.StartTime}");
            gene.TimeSlot = newTimeSlot;
        }
    }
}