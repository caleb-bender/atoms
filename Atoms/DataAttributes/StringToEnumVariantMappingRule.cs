using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalebBender.Atoms.DataAttributes
{
	/// <summary>
	/// If you have a string database property with unreadable or cryptic values
	/// that would work well within an enumeration, then you can apply a string to enum
	/// mapping rule with this attribute, which will map the string value in the database
	/// to the corresponding enum variant. On both reads and writes, the first matched
	/// mapping rule will be used. You may have as many of these attributes above your
	/// enumeration type as you need.
	/// </summary>
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
