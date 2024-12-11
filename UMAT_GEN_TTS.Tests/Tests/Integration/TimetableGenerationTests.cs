using Microsoft.AspNetCore.Mvc;
using Xunit;
using UMAT_GEN_TTS.Controllers;
using UMAT_GEN_TTS.Core.Models;
using UMAT_GEN_TTS.Core.Models.Preferences;
using UMAT_GEN_TTS.Core.Models.API;
using UMAT_GEN_TTS.Interfaces;
using UMAT_GEN_TTS.Services;
using UMAT_GEN_TTS.Core.Constraints;
using UMAT_GEN_TTS.Core.Debug;
using UMAT_GEN_TTS.Core.Interfaces;
using UMAT_GEN_TTS.Core.Validator;
using UMAT_GEN_TTS.Core.GeneticAlgorithms;
using UMAT_GEN_TTS.Core.Configuration;
using UMAT_GEN_TTS.Core.Helpers;

namespace UMAT_GEN_TTS.Tests.Integration;

public class TimetableGenerationTests
{
    [Fact]
    public async Task GenerateTimetable_ShouldCompleteWithinTimeout()
    {
         // Arrange
    var controller = CreateTimetableController();
    var request = CreateSampleRequest();
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

    // Act & Assert
    await Assert.ThrowsAsync<TimeoutException>(async () => 
    {
        await controller.GenerateTimetable(request, cts.Token);
    });
    }

    [Fact]
    public async Task GenerateTimetable_WithSmallDataset_ShouldSucceed()
    {
        // Arrange
        var controller = CreateTimetableController();
        var request = new TimetableRequest
        {
            Courses = new List<Course>
            {
                CreateSampleCourse("TEST101", 30, false),
                CreateSampleCourse("TEST102", 25, false)
            },
            Rooms = new List<Room>
            {
                CreateSampleRoom("R101", 50),
                CreateSampleRoom("R102", 40)
            }
        };

        // Act
        var result = await controller.GenerateTimetable(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<TimetableResponse>(okResult.Value);
        Assert.NotEmpty(response.Schedule.EntriesByDay);
        Assert.True(response.Fitness > 0, "Fitness should be greater than 0");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task GenerateTimetable_WithDifferentSizes_ShouldComplete(int courseCount)
    {
        // Arrange
        var controller = CreateTimetableController();
        var request = new TimetableRequest
        {
            Courses = Enumerable.Range(1, courseCount)
                .Select(i => CreateSampleCourse($"TEST{i}", 30, false))
                .ToList(),
            Rooms = new List<Room>
            {
                CreateSampleRoom("R101", 50),
                CreateSampleRoom("R102", 40)
            }
        };

        // Act
        var result = await controller.GenerateTimetable(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<TimetableResponse>(okResult.Value);
        
        // Check for basic validity
        Assert.NotEmpty(response.Schedule.EntriesByDay);
        Assert.True(response.Fitness >= 0.1, "Fitness should be at least 0.1");
        
        // Verify some schedule properties
        Assert.True(response.Schedule.EntriesByDay.Values.SelectMany(x => x).Count() > 0, 
            "Schedule should have at least one entry");
    }

    private static TimetableController CreateTimetableController()
    {
        var services = new ServiceCollection();
        
        // Add logging with console output
        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Information);
        });

        // Register test data
        var testRooms = CreateTestRooms();
        var testTimeSlots = CreateTestTimeSlots();

        services.AddSingleton(testRooms);
        services.AddSingleton(testTimeSlots);
        services.AddSingleton(new List<Course>());
        services.AddSingleton(new SystemConfiguration 
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
                            }
                        }
                    }
                }
            },
            AlgorithmSettings = new AlgorithmConfiguration
            {
                PopulationSize = 10,
                ElitismRate = 0.1,
                MutationRate = 0.1
            }
        });

        // Register core services
        services.AddSingleton<ITimetableService, TimetableService>();
        services.AddSingleton<IFitnessCalculator, ConstraintManager>();
        services.AddSingleton<ConstraintManager>();
        services.AddSingleton<TimetableValidator>();
        
        // Register GA components with factory methods
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

        // Add logging
        services.AddLogging();

        services.AddSingleton<TimetableDebugger>(sp => 
            new TimetableDebugger(
                sp.GetRequiredService<ILogger<TimetableDebugger>>(),
                sp.GetRequiredService<ConstraintManager>()
            ));

        var serviceProvider = services.BuildServiceProvider();
        return new TimetableController(
            serviceProvider.GetRequiredService<ITimetableService>(),
            serviceProvider.GetRequiredService<ILogger<TimetableController>>()
        );
    }

    private static Course CreateSampleCourse(string code, int studentCount, bool requiresLab)
    {
        var department = new Department { Name = "Test", Code = "TEST" };
        return new Course
        {
            Code = int.Parse(code.Replace("TEST", "")),
            Name = $"Test Course {code}",
            StudentCount = studentCount,
            RequiresLab = requiresLab,
            Mode = CourseMode.Regular,
            AssignedDepartment = department,
            Lecturer = new Lecturer { Name = $"Lecturer {code}", Department = department },
            WeeklyHours = 2,
            SessionsPerWeek = 1,
            ProgrammeYears = new List<Programme>
            {
                new() { Name = "Test Programme", Code = "TP", Department = department, Year = 1 }
            },
            Preferences = new CoursePreferences 
            { 
                PreferredTimeSlots = new Dictionary<TimeSlot, double>(),
                PreferredRooms = new Dictionary<Room, double>()
            }
        };
    }

    private static Room CreateSampleRoom(string name, int capacity)
    {
        return new Room
        {
            Name = name,
            MaxCapacity = capacity,
            IsLab = false,
            IsAvailable = true,
            Features = new List<string>(),
            Equipment = new List<string>()
        };
    }

    private static TimetableRequest CreateSampleRequest()
{
    return new TimetableRequest
    {
        Courses = new List<Course>
        {
            CreateSampleCourse("TEST101", 30, false)
        },
        Rooms = new List<Room>
        {
                CreateSampleRoom("R101", 50)
            }
        };
    }

    private static List<TimeSlot> CreateTestTimeSlots()
    {
        var timeSlots = new List<TimeSlot>();
        var days = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday };
        var startTimes = new[] { 9, 11, 14, 16 }; // Hours in 24-hour format

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

    private static List<Room> CreateTestRooms()
    {
        return new List<Room>
        {
            CreateSampleRoom("R101", 50),
            CreateSampleRoom("R102", 40)
        };
    }
} 