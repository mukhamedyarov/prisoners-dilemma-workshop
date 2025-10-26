namespace PrisonersDilemma.Api.Exceptions;

public sealed class InvalidRequestException : Exception
{
	public InvalidRequestException(string message) : base(message) { }

	public InvalidRequestException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}