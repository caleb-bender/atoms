using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Exceptions
{
	/// <summary>
	/// This exception occurs when Atoms fails to map an Enum type to a string,
	/// or conversely, a string to an Enum type.
	/// </summary>
	public class EnumPropertyMappingFailedException : AtomsException
	{
		public EnumPropertyMappingFailedException(string? message = null) : base(message) { }
		public EnumPropertyMappingFailedException(string? message, Exception? innerException) : base(message, innerException)
		{
		}
	}
}
