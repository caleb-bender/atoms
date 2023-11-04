using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Exceptions
{
	public class AtomsException : Exception
	{
		public AtomsException(string? message = null) : base(message) { }
		public AtomsException(string? message, Exception? innerException) : base(message, innerException)
		{
		}
	}
}
