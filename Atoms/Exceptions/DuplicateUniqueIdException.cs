using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Exceptions
{
	public class DuplicateUniqueIdException : AtomsException
	{
		public DuplicateUniqueIdException(string? message = null) : base(message) { }
		public DuplicateUniqueIdException(string? message, Exception? innerException) : base(message, innerException)
		{
		}
	}
}
