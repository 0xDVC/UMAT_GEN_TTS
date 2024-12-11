using UMAT_GEN_TTS.Core.Models;
using UMAT_GEN_TTS.Core.GeneticAlgorithms;
using UMAT_GEN_TTS.Core.Configuration;

namespace UMAT_GEN_TTS.Interfaces;

public interface ITimetableService
{
    Task<(Chromosome Solution, ValidationResult Result)> GenerateTimetable(
        List<Course> courses, List<Room> rooms);
    List<TimeSlot> GenerateTimeSlots();
    ValidationResult ValidateTimetable(Chromosome solution);
} 