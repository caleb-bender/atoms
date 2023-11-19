using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Exceptions
{
	/// <summary>
	/// This exception occurs when an IAtomicRepositoryFactory's data model class
	/// generic type does not contain the UniqueIdAttribute on at least one of its properties.
	/// </summary>
	public class MissingUniqueIdAttributeException : AtomsException
	{
		public MissingUniqueIdAttributeException(string? message = null) : base(message) { }
		public MissingUniqueIdAttributeException(string? message, Exception? innerException) : base(message, innerException)
		{
		}
	}
}
