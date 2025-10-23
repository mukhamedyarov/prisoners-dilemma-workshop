using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PrisonersDilemma.Api.Authorization;
using PrisonersDilemma.Api.Configuration;

namespace PrisonersDilemma.Api.Controllers;

[ApiController]
[Route("api/game")]
[ConditionalAuthorize]
public class GameController : ControllerBase
{
   private readonly ApiKeySettings _apiKeySettings;
    
    public GameController(IOptions<ApiKeySettings> apiKeySettings)
    {
        _apiKeySettings = apiKeySettings.Value;
    }
}