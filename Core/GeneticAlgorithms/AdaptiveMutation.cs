using UMAT_GEN_TTS.Core.Interfaces;
using UMAT_GEN_TTS.Core.Models;

namespace UMAT_GEN_TTS.Core.GeneticAlgorithms;

public class AdaptiveMutation : IMutationStrategy
{
    private readonly Random _random = new();
    private readonly double _baseMutationRate;
    private readonly double _adaptiveFactor;
    private double _currentMutationRate;
    private double _previousBestFitness;
    private readonly List<Room> _availableRooms;
    private readonly List<TimeSlot> _availableTimeSlots;

    public double BaseMutationRate { get; private set; }

    public AdaptiveMutation(List<Room> rooms, List<TimeSlot> timeSlots, 
        double baseMutationRate = 0.1, double adaptiveFactor = 0.5)
    {
        _availableRooms = rooms;
        _availableTimeSlots = timeSlots;
        _baseMutationRate = baseMutationRate;
        _adaptiveFactor = adaptiveFactor;
        _currentMutationRate = baseMutationRate;
        _previousBestFitness = 0;
        BaseMutationRate = baseMutationRate;
    }

    public void Mutate(Chromosome chromosome)
    {
        // Adjust mutation rate based on fitness improvement
        if (chromosome.Fitness > _previousBestFitness)
        {
            // Reduce mutation rate when improving
            _currentMutationRate *= (1 - _adaptiveFactor);
        }
        else
        {
            // Increase mutation rate when stuck
            _currentMutationRate *= (1 + _adaptiveFactor);
        }

        // Keep mutation rate within bounds
        _currentMutationRate = Math.Max(0.01, Math.Min(0.5, _currentMutationRate));
        _previousBestFitness = chromosome.Fitness;

        foreach (var gene in chromosome.Genes)
        {
            if (_random.NextDouble() > _currentMutationRate)
                continue;

            // Don't mutate room for virtual courses
            if (gene.Course.Mode == CourseMode.Virtual)
            {
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
        // Get suitable rooms based on course requirements
        var suitableRooms = _availableRooms
            .Where(r => r.MaxCapacity >= gene.Course.StudentCount)
            .Where(r => !gene.Course.RequiresLab || r.IsLab)
            .Where(r => r.AssignedDepartment == gene.Course.AssignedDepartment || 
                        r.Ownership == RoomOwnership.GeneralPurpose)
            .ToList();

        if (!suitableRooms.Any()) return;

        // Select new room randomly from suitable ones
        var newRoom = suitableRooms[_random.Next(suitableRooms.Count)];
        if (newRoom != gene.Room)  // Only change if different
        {
            gene.Room = newRoom;
            gene.IsCompromised = false;
        }
    }

    private void MutateTimeSlot(Gene gene)
    {
        // Get all possible time slots
        var availableSlots = _availableTimeSlots
            .Where(ts => !gene.Course.Preferences.DaysNotAvailable.Contains(ts.Day))
            .Where(ts => gene.Course.Preferences.PreferredSessionType == ts.Type)
            .ToList();

        if (!availableSlots.Any()) return;

        // Select new time slot randomly
        var newSlot = availableSlots[_random.Next(availableSlots.Count)];
        if (!newSlot.Equals(gene.TimeSlot))  // Only change if different
        {
            gene.TimeSlot = newSlot;
            gene.IsCompromised = false;
        }
    }

    public void SetCurrentRate(double rate)
    {
        BaseMutationRate = rate;
    }
} 