using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PrisonersDilemma.Api.Configuration;

namespace PrisonersDilemma.Api.Controllers;

[ApiController]
[Route("api/auth")]
[ApiExplorerSettings(IgnoreApi = true)]
public class AuthController : ControllerBase
{
    private readonly JwtSettings _jwtSettings;

    // Static dictionary of valid client credentials
    private static readonly Dictionary<string, string> ValidClients = new()
    {
        { "client-001", "a7b9c3d4e5f6789012345678901234567890abcd" },
        { "client-002", "b8c0d4e6f7890123456789012345678901bcde0123" },
        { "client-003", "c9d1e5f7890123456789012345678901cdef1234567" },
        { "client-004", "d0e2f6890123456789012345678901def23456789a" },
        { "client-005", "e1f3079012345678901234567890ef3456789ab01" },
        { "client-006", "f2408012345678901234567890f456789abc012345" },
        { "client-007", "051901234567890123456789056789abcd01234567" },
        { "client-008", "162a0123456789012345678906789abcde012345678" },
        { "client-009", "273b012345678901234567890789abcdef0123456789" },
        { "client-010", "384c01234567890123456789089abcdef01234567890" }
    };

    public AuthController(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    [HttpPost("token")]
    public IActionResult Token([FromForm] TokenRequest request)
    {
        if (!_jwtSettings.Enabled)
        {
            return BadRequest(new TokenErrorResponse
            {
                Error = "server_error",
                ErrorDescription = "JWT authentication is not enabled"
            });
        }

        // Validate grant_type for Client Credentials Flow
        if (request.GrantType != "client_credentials")
        {
            return BadRequest(new TokenErrorResponse
            {
                Error = "unsupported_grant_type",
                ErrorDescription = "Only client_credentials grant type is supported"
            });
        }

        // Validate client credentials
        if (string.IsNullOrEmpty(request.ClientId) || string.IsNullOrEmpty(request.ClientSecret))
        {
            return BadRequest(new TokenErrorResponse
            {
                Error = "invalid_request",
                ErrorDescription = "client_id and client_secret are required"
            });
        }

        // Validate client credentials against the static dictionary
        if (!ValidClients.TryGetValue(request.ClientId, out var expectedSecret) || 
            request.ClientSecret != expectedSecret)
        {
            return Unauthorized(new TokenErrorResponse
            {
                Error = "invalid_client",
                ErrorDescription = "Invalid client credentials"
            });
        }

        // Generate JWT token
        var token = GenerateJwtToken(request.ClientId, request.Scope);
        
        return Ok(new TokenResponse
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresIn = _jwtSettings.ExpirationMinutes * 60,
            Scope = request.Scope ?? string.Empty
        });
    }

    private string GenerateJwtToken(string clientId, string? scope = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new("client_id", clientId),
            new(JwtRegisteredClaimNames.Sub, clientId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        if (!string.IsNullOrEmpty(scope))
        {
            claims.Add(new Claim("scope", scope));
        }

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [HttpGet("user")]
    [Authorize]
    public IActionResult GetUser()
    {
        if (!_jwtSettings.Enabled)
        {
            return BadRequest("JWT authentication is not enabled");
        }

        var clientId = User.FindFirst("client_id")?.Value;
        var scope = User.FindFirst("scope")?.Value;

        return Ok(new
        {
            IsAuthenticated = true,
            ClientId = clientId,
            Scope = scope,
            Claims = User.Claims.Select(c => new { c.Type, c.Value })
        });
    }
}

// OAuth 2.0 Client Credentials Flow models
public class TokenRequest
{
    [FromForm(Name = "grant_type")]
    public string GrantType { get; set; } = string.Empty;
    
    [FromForm(Name = "client_id")]
    public string ClientId { get; set; } = string.Empty;
    
    [FromForm(Name = "client_secret")]
    public string ClientSecret { get; set; } = string.Empty;
    
    [FromForm(Name = "scope")]
    public string? Scope { get; set; }
}

public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;
    
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;
    
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    
    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;
}

public class TokenErrorResponse
{
    [JsonPropertyName("error")]
    public string Error { get; set; } = string.Empty;
    
    [JsonPropertyName("error_description")]
    public string ErrorDescription { get; set; } = string.Empty;
}