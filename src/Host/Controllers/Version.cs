using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Host.Controllers;

[Route("[controller]")]
[AllowAnonymous]
public class VersionController : ControllerBase
{
    [HttpGet(Name = "Version")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(Program.Version);
    }
}
