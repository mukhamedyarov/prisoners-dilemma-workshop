namespace PrisonersDilemma.Api.Exceptions;

public sealed class GameNotFoundException : Exception
{
    public Guid SessionId { get; }
    
    public GameNotFoundException(Guid sessionId) 
        : base($"Game session with ID {sessionId} was not found.")
    {
        SessionId = sessionId;
        Data.Add(nameof(sessionId), sessionId);
    }
}