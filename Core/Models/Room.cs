namespace UMAT_GEN_TTS.Core.Models;

public class Room
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public int Capacity { get; set; }
    public bool IsLab { get; set; }
    public string Building { get; set; }
    public string Department { get; set; }
    public RoomOwnership Ownership { get; set; }
    public LabType? LabType { get; set; }
    public List<string> Features { get; set; } = new();
}

public enum RoomOwnership
{
    Departmental,    // Only department can use
    Faculty,         // Priority to faculty departments
    GeneralPurpose   // Anyone can use
}

public enum LabType
{
    ComputerLab,     // Can be substituted with rooms with projectors
    ElectricalLab,   // Must be scheduled here
    ChemistryLab,    // Must be scheduled here
    PhysicsLab       // Must be scheduled here
}