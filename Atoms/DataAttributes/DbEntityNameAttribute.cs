using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.DataAttributes
{
	/// <summary>
	/// If the name of the database entity in question does not equal the plural of its
	/// corresponding data model class name (i.e. Address becomes Addresss not Addresses)
	/// then annotate the class with this attribute, specifiying the name of the database
	/// entity.
	/// </summary>
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
