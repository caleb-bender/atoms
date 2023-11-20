using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalebBender.Atoms.Exceptions
{
	/// <summary>
	/// AtomsException is the base class for all exceptions that originate from Atoms ORM.
	/// </summary>
	public class AtomsException : Exception
	{
		public AtomsException(string? message = null) : base(message) { }
		public AtomsException(string? message, Exception? innerException) : base(message, innerException)
		{
		}
	}
}
