using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Exceptions
{
	public class DbEntityNotFoundException : AtomsException
	{
		public DbEntityNotFoundException(string? message = null) : base(message) { }
		public DbEntityNotFoundException(string? message, Exception? innerException) : base(message, innerException)
		{
		}
	}
}
