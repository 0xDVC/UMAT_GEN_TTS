namespace UMAT_GEN_TTS.Core.Models;

public class Room
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public int Capacity { get; set; }
    public bool IsLab { get; set; }
    public List<string> Features { get; set; } = new();
} 