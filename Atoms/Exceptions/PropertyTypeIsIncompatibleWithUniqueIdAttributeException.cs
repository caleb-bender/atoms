using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Exceptions
{
	/// <summary>
	/// This exception occurs when a property of a data model class cannot be used as
	/// a unique id. Supported unique id types are all primitive types, strings, Enums,
	/// DateTimes, and Guids.
	/// </summary>
	public class PropertyTypeIsIncompatibleWithUniqueIdAttributeException : AtomsException
	{
		public PropertyTypeIsIncompatibleWithUniqueIdAttributeException(string? message = null) : base(message) { }
		public PropertyTypeIsIncompatibleWithUniqueIdAttributeException
		(string? message, Exception? innerException) : base(message, innerException)
		{
		}
	}
}
