using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Exceptions
{
	/// <summary>
	/// This exception occurs when the length of a data model class string property
	/// exceeds the maximum length allowed by its corresponding MaxLengthAttribute.
	/// </summary>
	public class StringPropertyValueExceedsMaxLengthException : AtomsException
	{
		public StringPropertyValueExceedsMaxLengthException(string? message = null) : base(message) { }
		public StringPropertyValueExceedsMaxLengthException(string? message, Exception? innerException) : base(message, innerException)
		{
		}
	}
}
