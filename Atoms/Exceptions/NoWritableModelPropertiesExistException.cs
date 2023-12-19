using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalebBender.Atoms.Exceptions
{
	/// <summary>
	/// This exception occurs when all of a data model's public properties are either
	/// auto-generated unique ids or ignored properties.
	/// Thus, no public property of the model is writable.
	/// </summary>
	public class NoWritableModelPropertiesExistException : AtomsException
	{
		public NoWritableModelPropertiesExistException(string? message = null) : base(message) { }
		public NoWritableModelPropertiesExistException(string? message, Exception? innerException) : base(message, innerException)
		{
		}
	}
}
