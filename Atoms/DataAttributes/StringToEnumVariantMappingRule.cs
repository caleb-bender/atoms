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
		public StringToEnumVariantMappingRule(string databaseStringValue, object enumVariant)
		{
			DatabaseStringValue = databaseStringValue;
			EnumVariant = enumVariant;
		}

		public string DatabaseStringValue { get; }
		public object EnumVariant { get; }
	}
}
