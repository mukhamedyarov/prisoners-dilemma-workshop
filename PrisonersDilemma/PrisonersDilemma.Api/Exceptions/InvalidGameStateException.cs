namespace PrisonersDilemma.Api.Exceptions;

public sealed class InvalidGameStateException : Exception
{
	public InvalidGameStateException(string message) : base(message) { }

	public InvalidGameStateException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}