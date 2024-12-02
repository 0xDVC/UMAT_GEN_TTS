using Microsoft.AspNetCore.Mvc;

namespace UMAT_GEN_TTS.Controllers;

[ApiController]
[Route("[controller]")]
public class TimetableController : ControllerBase
{


    // private readonly ILogger<TimetableController> _logger;

    // public TimetableController(ILogger<TimetableController> logger)
    // {
    //     _logger = logger;
    // }

    [HttpGet]
    public string Get()
    {
        return "Hello, World!";
    }
}
