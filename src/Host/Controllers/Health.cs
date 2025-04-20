using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Host.Controllers;

[Route("[controller]")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
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
            return StatusCode(StatusCodes.Status500InternalServerError, "☠️");
        }
    }
}
