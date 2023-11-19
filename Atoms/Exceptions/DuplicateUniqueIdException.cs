using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Exceptions
{
	/// <summary>
	/// This exception occurs when a data class model instance's unique id(s)
	/// already exist in the database entity, and thus cannot be used a second time.
	/// </summary>
	public class DuplicateUniqueIdException : AtomsException
	{
		public DuplicateUniqueIdException(string? message = null) : base(message) { }
		public DuplicateUniqueIdException(string? message, Exception? innerException) : base(message, innerException)
		{
		}
	}
}
