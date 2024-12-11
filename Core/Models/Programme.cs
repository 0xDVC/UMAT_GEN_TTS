namespace UMAT_GEN_TTS.Core.Models;

public class Programme
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; init; }
    public string Code { get; init; }
    public Department Department { get; init; }
    public int Year { get; init; }
    public int DurationYears { get; init; }
    public int MaxCreditsPerSemester { get; init; }
    public List<Course> CoreCourses { get; init; } = new();
    public List<Course> ElectiveCourses { get; init; } = new();
    public List<AcademicYear> Curriculum { get; init; } = new();
    public int MaxWeeklyHours { get; set; } = 30;
}


public class AcademicYear
{
    public int Year { get; init; }
    public List<Semester> Semesters { get; init; } = new();
}

public class Semester
{
    public int Number { get; init; }
    public List<Course> Courses { get; init; } = new();
    public int MinimumCredits { get; init; }
    public int MaximumCredits { get; init; }
}