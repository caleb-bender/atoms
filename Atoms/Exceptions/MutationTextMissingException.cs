
namespace CalebBender.Atoms.Exceptions
{
	public class MutationTextMissingException : AtomsException
	{
		public MutationTextMissingException(string? message = null) : base(message) { }
		public MutationTextMissingException(string? message, Exception? innerException) : base(message, innerException)
		{
		}
	}
}
