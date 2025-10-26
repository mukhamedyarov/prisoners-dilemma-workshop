namespace PrisonersDilemma.Api.Exceptions;

public sealed class GameNotFoundException : Exception
{
	public GameNotFoundException(string message) : base(message) { }

	public GameNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}