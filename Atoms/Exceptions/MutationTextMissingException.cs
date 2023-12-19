
namespace CalebBender.Atoms.Exceptions
{
	/// <summary>
	/// This exception occurs when a required mutation text is null or empty
	/// </summary>
	public class MutationTextMissingException : AtomsException
	{
		public MutationTextMissingException(string? message = null) : base(message) { }
		public MutationTextMissingException(string? message, Exception? innerException) : base(message, innerException)
		{
		}
	}
}
