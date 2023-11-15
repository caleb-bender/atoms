using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Exceptions
{
	public class StringPropertyValueExceedsMaxLengthException : AtomsException
	{
		public StringPropertyValueExceedsMaxLengthException(string? message = null) : base(message) { }
		public StringPropertyValueExceedsMaxLengthException(string? message, Exception? innerException) : base(message, innerException)
		{
		}
	}
}
