using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Exceptions
{
	public class PropertyTypeIsIncompatibleWithUniqueIdAttributeException : AtomsException
	{
		public PropertyTypeIsIncompatibleWithUniqueIdAttributeException(string? message = null) : base(message) { }
		public PropertyTypeIsIncompatibleWithUniqueIdAttributeException
		(string? message, Exception? innerException) : base(message, innerException)
		{
		}
	}
}
