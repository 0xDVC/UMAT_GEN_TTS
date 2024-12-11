using UMAT_GEN_TTS.Core.Models;

namespace UMAT_GEN_TTS.Core.Models;

public class TimetableCache
{
    public Dictionary<Guid, HashSet<Room>> ValidRooms { get; } = new();
    public Dictionary<Guid, HashSet<TimeSlot>> ValidTimeSlots { get; } = new();
    public Dictionary<Guid, HashSet<Guid>> ProgrammeConflicts { get; } = new();
    public Dictionary<string, HashSet<string>> DepartmentRooms { get; } = new();
} 