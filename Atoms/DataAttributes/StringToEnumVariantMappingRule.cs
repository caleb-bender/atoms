using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.DataAttributes
{
	[AttributeUsage(AttributeTargets.Enum, AllowMultiple = true)]
	public class StringToEnumVariantMappingRule : Attribute
	{
		public StringToEnumVariantMappingRule(string databasePropertyValue, object desiredEnumVariant)
		{
			DatabasePropertyValue = databasePropertyValue;
			DesiredEnumVariant = desiredEnumVariant;
		}

		public string DatabasePropertyValue { get; }
		public object DesiredEnumVariant { get; }
	}
}
