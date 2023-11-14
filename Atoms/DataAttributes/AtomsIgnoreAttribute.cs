using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.DataAttributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class AtomsIgnoreAttribute : Attribute
	{
		public bool ReadFromDatabase { get; init; } = false;
	}
}
