using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Exceptions
{
	public class ModelDbEntityMismatchException : AtomsException
	{
		public ModelDbEntityMismatchException(string? message = null) : base(message) { }
		public ModelDbEntityMismatchException(string? message, Exception? innerException) : base(message, innerException)
		{
		}
	}
}
