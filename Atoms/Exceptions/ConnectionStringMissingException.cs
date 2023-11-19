using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Exceptions
{
	/// <summary>
	/// This exception occurs when a required connection string is null or empty
	/// </summary>
	public class ConnectionStringMissingException : AtomsException
	{
		public ConnectionStringMissingException(string? message = null) : base(message) { }
		public ConnectionStringMissingException(string? message, Exception? innerException) : base(message, innerException)
		{
		}
	}
}
