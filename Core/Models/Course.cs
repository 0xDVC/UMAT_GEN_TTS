namespace UMAT_GEN_TTS.Core.Models;

public enum CourseMode
{
    Physical,
    Virtual,
    Hybrid
}

public class ProgrammeYear
{
    public string ProgrammeCode { get; }
    public int Year { get; }

    public ProgrammeYear(string programmeCode, int year)
    {
        ProgrammeCode = programmeCode;
        Year = year;
    }

    public override string ToString() => $"{ProgrammeCode}-Y{Year}";
}

public class Course
{
    public Guid Id { get; }
    public string Code { get; set; }
    public string Name { get; set; }
    public int StudentCount { get; set; }
    public bool RequiresLab { get; set; }
    public int WeeklyHours { get; set; }
    public CourseMode Mode { get; set; }
    public string Department { get; set; }
    public string LecturerId { get; set; }
    public List<ProgrammeYear> ProgrammeYears { get; }
    public List<string> RelatedCourseCodes { get; }

    public Course(
        string code, 
        string name, 
        int studentCount, 
        bool requiresLab, 
        int weeklyHours,
        CourseMode mode = CourseMode.Physical,
        string department = "",
        List<ProgrammeYear>? programmeYears = null,
        List<string>? relatedCourseCodes = null)
    {
        Id = Guid.NewGuid();
        Code = code;
        Name = name;
        StudentCount = studentCount;
        RequiresLab = requiresLab;
        WeeklyHours = weeklyHours;
        Mode = mode;
        Department = department;
        ProgrammeYears = programmeYears ?? new List<ProgrammeYear>();
        RelatedCourseCodes = relatedCourseCodes ?? new List<string>();
    }

    // Helper methods
    public bool IsMandatory => ProgrammeYears.Count > 1;
    
    public bool IsSameYearGroup => ProgrammeYears
        .Select(py => py.Year)
        .Distinct()
        .Count() == 1;

    public int YearGroup => IsSameYearGroup 
        ? ProgrammeYears.First().Year 
        : -1; // -1 indicates mixed years

    public bool HasProgrammeConflict(Course other)
    {
        // Check if any programme-year combination exists in both courses
        return ProgrammeYears.Any(py =>
            other.ProgrammeYears.Any(opy =>
                py.ProgrammeCode == opy.ProgrammeCode &&
                py.Year == opy.Year));
    }

    public override string ToString()
    {
        var programmes = string.Join(", ", ProgrammeYears);
        return $"{Code} ({Name}) - {programmes}";
    }
} 