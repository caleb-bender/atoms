using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Exceptions
{
	public class PropertyValueExceedsMaxLengthException : AtomsException
	{
		public PropertyValueExceedsMaxLengthException(string? message = null) : base(message) { }
		public PropertyValueExceedsMaxLengthException(string? message, Exception? innerException) : base(message, innerException)
		{
		}
	}
}
