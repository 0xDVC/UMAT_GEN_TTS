namespace UMAT_GEN_TTS.Core.Models;

public class Department
{
    public Guid Id { get; } = Guid.NewGuid(); 
    public string Name { get; init; }
    public string Code { get; init; }
    public List<Programme> Programmes { get; init; } = new();
    public List<Guid> LecturerIds { get; init; } = new();
    public bool IsActive { get; init; } = true;
}