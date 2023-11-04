using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Exceptions
{
	public class MissingUniqueIdAttributeException : AtomsException
	{
		public MissingUniqueIdAttributeException(string? message = null) : base(message) { }
		public MissingUniqueIdAttributeException(string? message, Exception? innerException) : base(message, innerException)
		{
		}
	}
}
