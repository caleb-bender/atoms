using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalebBender.Atoms.DataAttributes
{
	/// <summary>
	/// Annotate a property of a data model class to ignore the property when retrieving/saving. By default, the property is ignored on both reads and
	/// writes, however to enable reading only, set the property ReadFromDatabase to true on
	/// initialization.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class AtomsIgnoreAttribute : Attribute
	{
		public bool ReadFromDatabase { get; init; } = false;
	}
}
