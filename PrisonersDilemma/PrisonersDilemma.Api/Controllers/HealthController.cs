using Microsoft.AspNetCore.Mvc;

namespace PrisonersDilemma.Api.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
	[HttpGet("live")]
	public IActionResult GetLiveness()
	{
		return Ok(new { status = "alive", timestamp = DateTime.UtcNow });
	}

	[HttpGet("ready")]
	public IActionResult GetReadiness()
	{
		return Ok(new { status = "ready", timestamp = DateTime.UtcNow });
	}
}