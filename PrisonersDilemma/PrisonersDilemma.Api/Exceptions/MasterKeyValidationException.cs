namespace PrisonersDilemma.Api.Exceptions;

public sealed class MasterKeyValidationException : Exception
{
	public MasterKeyValidationException(string message) : base(message) { }

	public MasterKeyValidationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}