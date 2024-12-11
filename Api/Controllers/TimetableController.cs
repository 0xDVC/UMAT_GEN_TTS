using Microsoft.AspNetCore.Mvc;
using UMAT_GEN_TTS.Interfaces;
using UMAT_GEN_TTS.Core.Models.API;
using UMAT_GEN_TTS.Core.Models;
using UMAT_GEN_TTS.Core.GeneticAlgorithms;
using Microsoft.Extensions.Logging;

namespace UMAT_GEN_TTS.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TimetableController : ControllerBase
{
    private readonly ITimetableService _timetableService;
    private readonly ILogger<TimetableController> _logger;

    public TimetableController(ITimetableService timetableService, ILogger<TimetableController> logger)
    {
        _timetableService = timetableService;
        _logger = logger;
    }

    /// <summary>
    /// Generates a new timetable based on provided courses and rooms
    /// </summary>
    /// <param name="request">Timetable generation request containing courses and rooms</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed</param>
    /// <returns>Generated timetable with validation results</returns>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(TimetableResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TimetableResponse>> GenerateTimetable(
        [FromBody] TimetableRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Controller: Request received with {CourseCount} courses and {RoomCount} rooms", 
                request.Courses?.Count ?? 0, request.Rooms?.Count ?? 0);

            if (request.Courses == null || !request.Courses.Any())
            {
                _logger.LogError("No courses provided in request");
                return BadRequest("No courses provided");
            }

            if (request.Rooms == null || !request.Rooms.Any())
            {
                _logger.LogError("No rooms provided in request");
                return BadRequest("No rooms provided");
            }

            var result = await _timetableService.GenerateTimetable(request.Courses, request.Rooms);
            _logger.LogInformation("Controller: Generation completed");
            return Ok(new TimetableResponse
            {
                IsValid = result.Result.IsValid,
                Fitness = result.Result.FinalFitness,
                Schedule = ConvertToSchedule(result.Solution),
                Violations = result.Result.Violations,
                Metrics = result.Result.Metrics
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Controller: Unhandled error during generation");
            throw;
        }
    }

    /// <summary>
    /// Validates an existing timetable
    /// </summary>
    /// <param name="timetable">Timetable to validate</param>
    /// <returns>Validation results</returns>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<ValidationResult> ValidateTimetable(
        [FromBody] Chromosome timetable)
    {
        return Ok(_timetableService.ValidateTimetable(timetable));
    }

   private Schedule ConvertToSchedule(Chromosome solution)
{
    var entries = solution.Genes
        .SelectMany(g => g.Course.ProgrammeYears.Select(py => new ScheduleEntry
        {
            CourseCode = g.Course.Code.ToString(),
            CourseName = g.Course.Name,
            RoomName = g.Room?.Name ?? "Virtual",
            Day = (int) g.TimeSlot.Day,
            StartTime = g.TimeSlot.StartTime.ToString(@"hh\:mm\:ss"),
            EndTime = g.TimeSlot.EndTime.ToString(@"hh\:mm\:ss"),
            LecturerName = g.Course.Lecturer?.Name ?? "TBA",
            Programme = py.Name,
            Year = py.Year
        }))
        .OrderBy(e => e.Programme)
        .ThenBy(e => e.Year)
        .ThenBy(e => e.StartTime)
        .ToList();

        return new Schedule
        {
            EntriesByDay = entries.GroupBy(e => e.Day)
                             .ToDictionary(g => g.Key, g => g.ToList())
        };
    }
}
