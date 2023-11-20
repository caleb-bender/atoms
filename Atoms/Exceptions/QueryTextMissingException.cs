using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalebBender.Atoms.Exceptions
{
	/// <summary>
	/// This exception occurs when a required query text is null or empty
	/// </summary>
	public class QueryTextMissingException : AtomsException
	{
		public QueryTextMissingException(string? message = null) : base(message) { }
		public QueryTextMissingException(string? message, Exception? innerException) : base(message, innerException)
		{
		}
	}
}
