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
    public Guid Id { get; } = Guid.NewGuid();
    public string Code { get; }
    public string Name { get; }
    public int StudentCount { get; }
    public bool RequiresLab { get; }
    public int CreditHours { get; }
    public int SessionsPerWeek { get; }
    public int HoursPerSession { get; }
    public CourseMode Mode { get; }
    public string Department { get; }
    public List<ProgrammeYear> ProgrammeYears { get; }
    public List<string> RelatedCourseCodes { get; }
    public string LecturerId { get; }

    public Course(
        string code, 
        string name, 
        int studentCount, 
        bool requiresLab,
        int creditHours,
        int sessionsPerWeek,
        int hoursPerSession,
        CourseMode mode = CourseMode.Physical,
        string department = "",
        string lecturerId = "",
        List<ProgrammeYear>? programmeYears = null,
        List<string>? relatedCourseCodes = null)
    {
        if (sessionsPerWeek * hoursPerSession < creditHours)
            throw new ArgumentException("Total weekly contact hours must be at least equal to credit hours");

        if (hoursPerSession > 2)
            throw new ArgumentException("No single session should exceed 2 hours");

        Code = code;
        Name = name;
        StudentCount = studentCount;
        RequiresLab = requiresLab;
        CreditHours = creditHours;
        SessionsPerWeek = sessionsPerWeek;
        HoursPerSession = hoursPerSession;
        Mode = mode;
        Department = department;
        LecturerId = lecturerId;
        ProgrammeYears = programmeYears ?? new();
        RelatedCourseCodes = relatedCourseCodes ?? new();
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

    public bool CanUseRoom(Room room)
    {
        // Virtual courses don't need rooms
        if (Mode == CourseMode.Virtual) return false;

        // Check capacity
        if (room.Capacity < StudentCount) return false;

        // Check lab requirements
        if (RequiresLab)
        {
            if (!room.IsLab) return false;
            
            // For computer labs, allow rooms with projectors as fallback
            if (room.LabType == LabType.ComputerLab && !room.Features.Contains("Projector"))
                return false;
        }

        // Check room ownership rules
        return room.Ownership switch
        {
            RoomOwnership.Departmental => room.Department == Department,
            RoomOwnership.Faculty => IsSameFaculty(room.Department),
            RoomOwnership.GeneralPurpose => true,
            _ => false
        };
    }

    private bool IsSameFaculty(string otherDepartment)
    {
        // Example faculty groupings - adjust based on your university structure
        var facultyGroups = new Dictionary<string, HashSet<string>>
        {
            ["Engineering"] = new() { "Computer Science", "Electrical", "Mechanical" },
            ["Science"] = new() { "Mathematics", "Physics", "Chemistry" }
        };

        return facultyGroups.Values.Any(faculty => 
            faculty.Contains(Department) && faculty.Contains(otherDepartment));
    }
} 