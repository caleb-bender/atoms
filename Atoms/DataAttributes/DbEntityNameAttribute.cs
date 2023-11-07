using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.DataAttributes
{
	[AttributeUsage(AttributeTargets.Class)]
	public class DbEntityNameAttribute : Attribute
	{
		public DbEntityNameAttribute(string name)
		{
			Name = name;
		}
		public string Name { get; }
	}
}
