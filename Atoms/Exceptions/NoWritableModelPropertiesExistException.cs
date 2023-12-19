using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalebBender.Atoms.Exceptions
{
	public class NoWritableModelPropertiesExistException : AtomsException
	{
		public NoWritableModelPropertiesExistException(string? message = null) : base(message) { }
		public NoWritableModelPropertiesExistException(string? message, Exception? innerException) : base(message, innerException)
		{
		}
	}
}
