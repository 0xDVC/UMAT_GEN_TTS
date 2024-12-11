namespace UMAT_GEN_TTS.Core.Models;

public record ValidationResult
{
    public bool IsValid { get; init; }
    public double FinalFitness { get; init; }
    public List<string> Violations { get; init; } = new();
    public Dictionary<string, double> Metrics { get; init; } = new();
} 