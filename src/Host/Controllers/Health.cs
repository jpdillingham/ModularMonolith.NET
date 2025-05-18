using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Host.Controllers;

[Route("[controller]")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
    private ILogger Log { get; } = Serilog.Log.ForContext<HealthController>();

    [HttpGet(Name = "Health")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public IActionResult Get()
    {
        try
        {
            // health check logic here :)
            return Ok("🚀");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Health check failed: {Message}", ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, "☠️");
        }
    }
}
