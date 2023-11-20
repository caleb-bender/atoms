using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalebBender.Atoms.Exceptions
{
	/// <summary>
	/// This exception
	/// </summary>
	public class ModelPropertyTypeMismatchException : AtomsException
	{
		public ModelPropertyTypeMismatchException(string? message = null) : base(message) { }
		public ModelPropertyTypeMismatchException(string? message, Exception? innerException) : base(message, innerException)
		{
		}
	}
}
