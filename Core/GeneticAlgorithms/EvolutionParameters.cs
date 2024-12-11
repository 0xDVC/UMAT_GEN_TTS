using UMAT_GEN_TTS.Core.Models;

namespace UMAT_GEN_TTS.Core.GeneticAlgorithms;

public class EvolutionParameters
{
    public List<Course> Courses { get; set; }
    public List<TimeSlot> AvailableSlots { get; set; }
    public List<Room> AvailableRooms { get; set; }
    public int MaxGenerations { get; set; }
    public double TargetFitness { get; set; }
} 