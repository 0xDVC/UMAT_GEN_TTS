namespace UMAT_GEN_TTS.Core.Models;

public class Room
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; init; }
    public string Building { get; init; }
    public string Floor { get; init; }
    public int MinCapacity { get; init; }
    public int MaxCapacity { get; init; }
    public bool IsLab { get; init; }
    public LabType? LabType { get; init; }
    public RoomOwnership Ownership { get; init; }
    public Department AssignedDepartment { get; init; }
    public List<string> Features { get; init; } = new();
    public List<string> Equipment { get; init; } = new();
    public List<TimeSlot> AvailableSlots { get; private set; } = new();
    public int PreferredGroupSize { get; init; }
    public bool IsAvailable { get; set; } = true;

    public bool IsAvailableForTimeSlot(TimeSlot slot)
    {
        if (!IsAvailable) return false;
        if (!AvailableSlots.Any()) return true;
        return !AvailableSlots.Any(s => s.Overlaps(slot));
    }

    public bool IsAvailableForCourse(Course course, TimeSlot slot)
    {
        return IsAvailableForTimeSlot(slot) && 
               MaxCapacity >= course.StudentCount &&
               (!course.RequiresLab || IsLab) &&
               (AssignedDepartment == course.AssignedDepartment || 
                Ownership == RoomOwnership.GeneralPurpose);
    }

    public static List<Room> GetAvailableRooms(IEnumerable<Room> rooms, Course course, TimeSlot slot)
    {
        return rooms
            .Where(r => r.IsAvailableForCourse(course, slot))
            .OrderBy(r => Math.Abs(r.PreferredGroupSize - course.StudentCount))
            .ToList();
    }
}

public enum RoomOwnership
{
    Departmental,    
    GeneralPurpose
}

public enum LabType
{
    ComputerLab,
    ElectricalLab,
    MiningLab,
    MineralsLab,
    GeologyLab,
    GeomaticsLab,
    MechanicalLab,
    ChemistryLab
}
