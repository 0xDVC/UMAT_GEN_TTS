using UMAT_GEN_TTS.Core.Models;
using UMAT_GEN_TTS.Core.GeneticAlgorithms;
using UMAT_GEN_TTS.Core.Configuration;
using UMAT_GEN_TTS.Core.Validator;
using UMAT_GEN_TTS.Interfaces;
using UMAT_GEN_TTS.Exceptions;


namespace UMAT_GEN_TTS.Services;

public class TimetableService : ITimetableService
{
    private readonly TimetableGeneticAlgorithm _algorithm;
    private readonly TimetableValidator _validator;
    private readonly ILogger<TimetableService> _logger;
    private readonly SystemConfiguration _config;

    public TimetableService(
        TimetableGeneticAlgorithm algorithm,
        TimetableValidator validator,
        SystemConfiguration config,
        ILogger<TimetableService> logger)
    {
        _algorithm = algorithm;
        _validator = validator;
        _config = config;
        _logger = logger;
    }

    public async Task<(Chromosome Solution, ValidationResult Result)> GenerateTimetable(
        List<Course> courses, List<Room> rooms)
    {
        _logger.LogInformation("Service: Starting generation with {CourseCount} courses and {RoomCount} rooms", 
            courses.Count, rooms.Count);

        ValidateInputs(courses, rooms);

        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5)); // 5-minute timeout

        try
        {
            _logger.LogInformation("Starting timetable generation");
            _logger.LogInformation($"Courses: {courses.Count}, Rooms: {rooms.Count}");

            var timeSlots = GenerateTimeSlots();
            _logger.LogInformation($"Time slots generated: {timeSlots.Count}");

            var parameters = new EvolutionParameters
            {
                Courses = courses,
                AvailableRooms = rooms,
                AvailableSlots = timeSlots,
                MaxGenerations = _config.AlgorithmSettings.MaxGenerations,
                TargetFitness = _config.AlgorithmSettings.TargetFitness
            };

            var solution = await Task.Run(() => _algorithm.Evolve(parameters), cts.Token);
            var validation = _validator.ValidateSolution(solution);

            _logger.LogInformation($"Generation complete. Final fitness: {validation.FinalFitness:F2}");
            if (!validation.IsValid)
            {
                _logger.LogWarning("Violations found:");
                foreach (var violation in validation.Violations)
                {
                    _logger.LogWarning($"- {violation}");
                }
            }

            return (Solution: solution, Result: validation);
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("Timetable generation timed out after 5 minutes");
            throw new TimeoutException("Timetable generation took too long");
        }
    }

    public ValidationResult ValidateTimetable(Chromosome solution)
    {
        return _validator.ValidateSolution(solution);
    }

    public List<TimeSlot> GenerateTimeSlots()
    {
        _logger.LogInformation("Generating time slots...");
        
        var slots = new List<TimeSlot>();
        foreach (var dayConfig in _config.TimeSettings.DayConfigurations)
        {
            _logger.LogInformation($"Processing day: {dayConfig.Key}");
            
            var day = dayConfig.Key;
            var config = dayConfig.Value;
            
            foreach (var duration in config.AllowedDurations)
            {
                var currentTicks = config.StartTime.Ticks;
                var endTicks = config.EndTime.Ticks;
                
                if (currentTicks >= endTicks)
                {
                    _logger.LogWarning($"Invalid time range for day {day}: Start {currentTicks} >= End {endTicks}");
                    continue;
                }

                while (currentTicks < endTicks)
                {
                    if (slots.Count > 1000)  // Arbitrary limit
                    {
                        _logger.LogWarning("Too many slots generated, breaking loop");
                        break;
                    }
                    
                    var currentTime = TimeSpan.FromTicks(currentTicks);
                    var slotDuration = TimeSpan.FromTicks(duration.Ticks);

                    if (!IsBreakPeriod(currentTime, slotDuration, config.Breaks))
                    {
                        slots.Add(new TimeSlot(
                            day, 
                            currentTime, 
                            currentTime.Add(slotDuration),
                            SessionType.Lecture,
                            false
                        ));
                    }
                    currentTicks += duration.Ticks;
                }
            }
        }
        
        _logger.LogInformation($"Generated {slots.Count} time slots");
        return slots;
    }

    static bool IsBreakPeriod(TimeSpan startTime, TimeSpan duration, List<Break> breaks)
    {
        var endTime = startTime.Add(duration);
        return breaks.Any(b => 
        {
            var breakStart = b.StartTime.ToTimeSpan();
            var breakEnd = breakStart.Add(b.Duration.ToTimeSpan());
            return (startTime >= breakStart && startTime < breakEnd) ||
                   (endTime > breakStart && endTime <= breakEnd);
        });
    }

    private void ValidateInputs(List<Course> courses, List<Room> rooms)
    {
        _logger.LogInformation("Validating inputs...");
        
        if (!courses.Any())
            throw new ArgumentException("No courses provided");

        if (!rooms.Any())
            throw new ArgumentException("No rooms provided");

        foreach (var course in courses)
        {
            _logger.LogInformation($"Validating course {course.Code}:");
            _logger.LogInformation($"- Mode: {course.Mode}");
            _logger.LogInformation($"- Students: {course.StudentCount}");
            _logger.LogInformation($"- Programme Years: {course.ProgrammeYears.Count}");
            
            if (course.ProgrammeYears.Count == 0)
                throw new ArgumentException($"Course {course.Code} has no programme years");
            
            if (course.Mode != CourseMode.Virtual && !rooms.Any(r => r.MaxCapacity >= course.StudentCount))
                throw new ArgumentException($"No suitable rooms for course {course.Code} (Students: {course.StudentCount})");
        }

        var timeSlots = GenerateTimeSlots();
        _logger.LogInformation($"Generated {timeSlots.Count} time slots");
        
        if (timeSlots.Count < courses.Count)
            throw new ArgumentException($"Insufficient time slots ({timeSlots.Count}) for courses ({courses.Count})");
    }
}