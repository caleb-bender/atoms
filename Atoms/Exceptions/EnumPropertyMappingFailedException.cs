using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Exceptions
{
	public class EnumPropertyMappingFailedException : AtomsException
	{
		public EnumPropertyMappingFailedException(string? message = null) : base(message) { }
		public EnumPropertyMappingFailedException(string? message, Exception? innerException) : base(message, innerException)
		{
		}
	}
}
