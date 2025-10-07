using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using PrisonersDilemma.Api.Configuration;

namespace PrisonersDilemma.Api.Authorization;

public class ConditionalAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var jwtSettings = context.HttpContext.RequestServices
            .GetRequiredService<IOptions<JwtSettings>>().Value;
        
        // If JWT is enabled, check if user is authenticated
        if (jwtSettings.Enabled)
        {
            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new ChallengeResult();
                return;
            }
        }
        // If JWT is disabled, do nothing - other validations (like master key) remain unchanged
    }
}