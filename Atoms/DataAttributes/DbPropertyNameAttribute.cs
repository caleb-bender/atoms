using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.DataAttributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class DbPropertyNameAttribute : Attribute
	{
		public DbPropertyNameAttribute(string name)
		{
			Name = name;
		}

		public string Name { get; }
	}
}
