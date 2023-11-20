using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalebBender.Atoms.Exceptions
{
	/// <summary>
	/// This exception occurs when one or more properties on a data model class
	/// do not map to corresponding database entity properties.
	/// </summary>
	public class ModelDbEntityMismatchException : AtomsException
	{
		public ModelDbEntityMismatchException(string? message = null) : base(message) { }
		public ModelDbEntityMismatchException(string? message, Exception? innerException) : base(message, innerException)
		{
		}
	}
}
