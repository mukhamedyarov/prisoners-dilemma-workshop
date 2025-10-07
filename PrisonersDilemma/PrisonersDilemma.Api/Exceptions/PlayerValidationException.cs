namespace PrisonersDilemma.Api.Exceptions;

public sealed class PlayerValidationException : Exception
{
    public int? PlayerNumber { get; }
    
    public PlayerValidationException(string message) : base(message) { }
    
    public PlayerValidationException(int playerNumber, string message) : base(message)
    {
        PlayerNumber = playerNumber;
        Data.Add(nameof(playerNumber), playerNumber);
    }
    
    public PlayerValidationException(string message, Exception innerException) 
        : base(message, innerException) { }
}