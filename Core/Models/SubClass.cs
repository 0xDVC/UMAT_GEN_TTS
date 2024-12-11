namespace UMAT_GEN_TTS.Core.Models;

public class SubClass
{
    public Guid Id { get; } = Guid.NewGuid();
    public string GroupId { get; init; }  // e.g., "A", "B"
    public int Size { get; init; }
    public List<Course> EnrolledCourses { get; init; }
    public Programme Programme { get; init; }
    public int Year { get; init; }
    public string Name => $"{Programme.Code} {Year}{GroupId}";  // e.g., "CE 3A"

    public override bool Equals(object? obj)
    {
        if (obj is not SubClass other) return false;
        return GroupId == other.GroupId && Programme.Code == other.Programme.Code;
    }

    public override int GetHashCode() => HashCode.Combine(GroupId, Programme.Code);
} 