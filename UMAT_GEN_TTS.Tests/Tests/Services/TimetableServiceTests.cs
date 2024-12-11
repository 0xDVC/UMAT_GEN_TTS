using Xunit;
using UMAT_GEN_TTS.Services;
using UMAT_GEN_TTS.Core.Configuration;
using UMAT_GEN_TTS.Core.GeneticAlgorithms;
using UMAT_GEN_TTS.Core.Validator;
using UMAT_GEN_TTS.Core.Helpers;
using Moq;
using UMAT_GEN_TTS.Core.Constraints;
using UMAT_GEN_TTS.Core.Interfaces;
using UMAT_GEN_TTS.Core.Models;
using UMAT_GEN_TTS.Core.Models.Preferences;
using UMAT_GEN_TTS.Core.Debug;
using UMAT_GEN_TTS.Interfaces;


namespace UMAT_GEN_TTS.Tests.Tests.Services;

public class TestTimetableValidator : TimetableValidator
{
    public TestTimetableValidator() : base(new Mock<ConstraintManager>().Object)
    {
    }

    public override ValidationResult ValidateSolution(Chromosome solution)
    {
        return new ValidationResult
        {
            IsValid = true,
            FinalFitness = 1.0,
            Violations = new List<string>(),
            Metrics = new Dictionary<string, double>
            {
                { "RoomUtilization", 0.8 },
                { "TimeSlotUtilization", 0.9 },
                { "HardConstraints", 1.0 },
                { "SoftConstraints", 0.9 }
            }
        };
    }
}

public class TestTimetableGeneticAlgorithm : TimetableGeneticAlgorithm 
{
    public TestTimetableGeneticAlgorithm() : base(
        new PopulationManager(
            new Mock<IFitnessCalculator>().Object, 
            new List<Course>(), 
            10, 
            0.1, 
            0.001),
        new Mock<ISelectionStrategy>().Object,
        new Mock<ICrossoverStrategy>().Object,
        new Mock<IMutationStrategy>().Object,
        new Mock<IFitnessCalculator>().Object,
        new TestTimetableValidator(),
        new Mock<ILogger<TimetableGeneticAlgorithm>>().Object,
        new TimetableDebugger(
            new Mock<ILogger<TimetableDebugger>>().Object,
            new Mock<ConstraintManager>().Object),
        maxGenerations: 100,
        targetFitness: 0.95,
        stagnationLimit: 20,
        convergenceThreshold: 0.001)
    { }
}

public class TimetableServiceTests
{
    private readonly SystemConfiguration _config;
    private readonly TestTimetableValidator _validator;
    private readonly Mock<ILogger<TimetableService>> _logger;
    private readonly TimetableService _service;

    public TimetableServiceTests()
    {
        // Setup common test configuration
        _config = new SystemConfiguration
        {
            TimeSettings = new TimeConfiguration
            {
                DayConfigurations = new Dictionary<DayOfWeek, DayConfig>
                {
                    {
                        DayOfWeek.Monday,
                        new DayConfig
                        {
                            StartTime = new TickTime { Ticks = TimeHelper.Times.SEVEN_AM },
                            EndTime = new TickTime { Ticks = TimeHelper.Times.FIVE_PM },
                            AllowedDurations = new List<TickTime>
                            {
                                new() { Ticks = TimeHelper.Times.ONE_HOUR },
                                new() { Ticks = TimeHelper.Times.TWO_HOURS }
                            },
                            Breaks = new List<Break>
                            {
                                new()
                                {
                                    StartTime = new TickTime { Ticks = TimeHelper.Times.ONE_PM },
                                    Duration = new TickTime { Ticks = TimeHelper.Times.THIRTY_MINUTES }
                                }
                            }
                        }
                    }
                }
            }
        };

        _validator = new TestTimetableValidator();
        _logger = new Mock<ILogger<TimetableService>>();
        _service = new TimetableService(
            new TestTimetableGeneticAlgorithm(),
            _validator,
            _config,
            _logger.Object
        );
    }

    [Fact]
    public void GenerateTimeSlots_ShouldCreateValidSlots()
    {
        // Act
        var slots = _service.GenerateTimeSlots();

        // Assert
        Assert.NotEmpty(slots);
        Assert.All(slots, slot =>
        {
            Assert.True(slot.StartTime >= TimeSpan.FromHours(7));
            Assert.True(slot.EndTime <= TimeSpan.FromHours(17));
        });
    }

    [Fact]
    public void GenerateTimeSlots_ShouldExcludeBreakTimes()
    {
        // Act
        var slots = _service.GenerateTimeSlots();

        // Assert
        Assert.All(slots, slot =>
        {
            var breakStart = TimeSpan.FromHours(13); // 1 PM
            var breakEnd = TimeSpan.FromHours(13.5); // 1:30 PM
            Assert.False(slot.Overlaps(new TimeSlot(
                DayOfWeek.Monday, 
                breakStart, 
                breakEnd)));
        });
    }

    [Fact]
    public async Task GenerateTimetable_WithValidInput_ShouldReturnValidSolution()
    {
        // Arrange
        var department = new Department 
        { 
            Name = "Test Department",
            Code = "TEST"
        };

        var lecturer = new Lecturer
        {
            Name = "Test Lecturer",
            StaffId = "TEST001",
            Department = department
        };

        var courses = new List<Course>
        {
            new Course
            {
                Code = 101,
                Name = "Test Course",
                StudentCount = 30,
                RequiresLab = false,
                Mode = CourseMode.Regular,
                AssignedDepartment = department,
                Lecturer = lecturer,
                WeeklyHours = 2,
                SessionsPerWeek = 1,
                SessionDuration = 2,
                ProgrammeYears = new List<Programme>
                {
                    new Programme
                    {
                        Name = "Test Programme",
                        Code = "TP",
                        Department = department,
                        Year = 1
                    }
                },
                Preferences = new CoursePreferences
                {
                    PreferredDays = new List<DayOfWeek> { DayOfWeek.Monday },
                    DaysNotAvailable = new List<DayOfWeek> { DayOfWeek.Sunday },
                    PreferredSessionType = SessionType.Lecture
                }
            }
        };

        var rooms = new List<Room>
        {
            new Room
            {
                Name = "Test Room",
                MaxCapacity = 50,
                IsLab = false,
                Building = "Test Building",
                Floor = "Ground",
                Features = new List<string> { "Projector" },
                Equipment = new List<string> { "Whiteboard" },
                PreferredGroupSize = 40,
                IsAvailable = true
            }
        };

        // Act
        var (solution, validation) = await _service.GenerateTimetable(courses, rooms);

        // Assert
        Assert.NotNull(solution);
        Assert.True(validation.IsValid);
        Assert.Equal(courses.Count, solution.Genes.Count);
    }

    private ITimetableService CreateTimetableService()
    {
        var services = new ServiceCollection();
        
        // Create test data
        var testRooms = new List<Room> { CreateSampleRoom("R101", 50) };
        var testTimeSlots = CreateTestTimeSlots();

        // Register services
        services.AddSingleton<IFitnessCalculator, ConstraintManager>();
        services.AddSingleton<TimetableValidator>();
        services.AddLogging();

        // Register GA components
        services.AddSingleton<PopulationManager>(sp => new PopulationManager(
            sp.GetRequiredService<IFitnessCalculator>(),
            new List<Course>(),
            10,
            0.1,
            0.001,
            rooms: testRooms,
            timeSlots: testTimeSlots
        ));

        services.AddSingleton<ISelectionStrategy, TournamentSelection>();
        services.AddSingleton<ICrossoverStrategy, MultiPointCrossover>();
        services.AddSingleton<IMutationStrategy>(sp => new AdaptiveMutation(
            rooms: testRooms,
            timeSlots: testTimeSlots,
            baseMutationRate: 0.1,
            adaptiveFactor: 0.5
        ));

        services.AddSingleton<TimetableGeneticAlgorithm>();
        services.AddSingleton<ITimetableService, TimetableService>();

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<ITimetableService>();
    }

    private static List<TimeSlot> CreateTestTimeSlots()
    {
        var timeSlots = new List<TimeSlot>();
        var days = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday };
        var startTimes = new[] { 9, 11, 14, 16 };

        foreach (var day in days)
        {
            foreach (var startHour in startTimes)
            {
                timeSlots.Add(new TimeSlot(
                    day,
                    TimeSpan.FromHours(startHour),
                    TimeSpan.FromHours(startHour + 2),
                    SessionType.Lecture,
                    true
                ));
            }
        }
        return timeSlots;
    }
}

