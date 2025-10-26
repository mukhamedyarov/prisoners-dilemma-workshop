using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using PrisonersDilemma.Api.Application.Interfaces;
using PrisonersDilemma.Api.Application.Services;
using PrisonersDilemma.Api.Configuration;
using PrisonersDilemma.Api.Data;
using PrisonersDilemma.Api.Infrastructure.Repositories;
using PrisonersDilemma.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddDbContext<GameDbContext>(options =>
	options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=PrisonersDilemma.db"));

builder.Services.AddScoped<IGameSessionRepository, GameSessionRepository>();
builder.Services.AddScoped<IGameService, GameService>();

builder.Services.Configure<ApiKeySettings>(
	builder.Configuration.GetSection(ApiKeySettings.SectionName));

builder.Services.Configure<JwtSettings>(
	builder.Configuration.GetSection(JwtSettings.SectionName));

// Configure JWT Bearer authentication if enabled
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();
if (jwtSettings?.Enabled == true)
{
	var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

	builder.Services.AddAuthentication(options =>
		{
			options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
		})
		.AddJwtBearer(options =>
		{
			options.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuer = jwtSettings.ValidateIssuer,
				ValidateAudience = jwtSettings.ValidateAudience,
				ValidateLifetime = jwtSettings.ValidateLifetime,
				ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey,
				ValidIssuer = jwtSettings.Issuer,
				ValidAudience = jwtSettings.Audience,
				IssuerSigningKey = new SymmetricSecurityKey(key),
				ClockSkew = TimeSpan.Zero
			};
		});

	builder.Services.AddAuthorization();
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
	var context = scope.ServiceProvider.GetRequiredService<GameDbContext>();
	context.Database.EnsureCreated();
}

app.UseExceptionHandler();

app.UseMiddleware<MasterKeyValidationMiddleware>();

app.MapOpenApi();
app.UseSwaggerUI(options =>
{
	options.SwaggerEndpoint("/p-dilemma/openapi/v1.json", "Prisoner's Dilemma API v1.1");
});

// app.UseHttpsRedirection();

// Add authentication and authorization middleware if JWT is enabled
if (jwtSettings?.Enabled == true)
{
	app.UseAuthentication();
	app.UseAuthorization();
}

app.MapControllers();

app.Run();