namespace PrisonersDilemma.Api.Exceptions;

public sealed class RoundNotFoundException : Exception
{
	public RoundNotFoundException(string message) : base(message) { }

	public RoundNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}