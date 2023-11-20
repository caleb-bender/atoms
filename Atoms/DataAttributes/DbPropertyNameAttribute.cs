using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalebBender.Atoms.DataAttributes
{
	/// <summary>
	/// If one or more property names in your data model class
	/// differ from the corresponding property names of the database entity,
	/// then you may annotate them with this attribute.
	/// </summary>
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
