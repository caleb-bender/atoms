using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Exceptions
{
	/// <summary>
	/// This exception occurs when the database entity name used by Atoms
	/// does not actually exist. Use DbEntityNameAttribute if the plural of your
	/// data model class name differs from the corresponding database entity name.
	/// </summary>
	public class DbEntityNotFoundException : AtomsException
	{
		public DbEntityNotFoundException(string? message = null) : base(message) { }
		public DbEntityNotFoundException(string? message, Exception? innerException) : base(message, innerException)
		{
		}
	}
}
