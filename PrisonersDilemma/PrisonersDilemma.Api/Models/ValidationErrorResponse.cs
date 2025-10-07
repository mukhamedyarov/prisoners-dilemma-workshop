namespace PrisonersDilemma.Api.Models;

public class ValidationErrorResponse
{
    public string Title { get; set; } = "Validation Failed";
    public string Detail { get; set; } = "One or more validation errors occurred.";
    public int Status { get; set; } = 400;
    public string? TraceId { get; set; }
    public string? Instance { get; set; }
    public Dictionary<string, string[]> Errors { get; set; } = new();
}