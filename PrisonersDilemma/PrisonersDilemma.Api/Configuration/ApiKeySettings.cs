namespace PrisonersDilemma.Api.Configuration;

public class ApiKeySettings
{
    public const string SectionName = "ApiKey";
    
    public string MasterKey { get; set; } = string.Empty;
}