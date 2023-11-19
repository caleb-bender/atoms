using Atoms.DataAttributes;
using Atoms.Repositories.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Utils.Reflection.TypeMapping
{
	internal static class EnumMappingHelpers
	{
		internal enum EnumParsingStatus
		{
			ParsingSuccessful,
			ParsingFailedBecauseStringIsInvalid,
			ParsingFailedBecauseOneOrMoreTypesAreIncompatible
		}

		internal static (EnumParsingStatus, object?) AttemptToParseEnumProperty(PropertyInfo property, object? value)
		{
			var propertyType = property.PropertyType;
			return EnumParsingStatusAndParsedValue(propertyType, value);
		}
		internal static (EnumParsingStatus, object?) AttemptToParseEnumField(FieldInfo field, object? value)
		{
			var fieldType = field.FieldType;
			return EnumParsingStatusAndParsedValue(fieldType, value);
		}

		private static (EnumParsingStatus, object?) EnumParsingStatusAndParsedValue(Type type, object? value)
		{
			var (parsingStatus, parsedEnum) = AttemptToParseEnum(type, value);
			if (parsingStatus != EnumParsingStatus.ParsingFailedBecauseStringIsInvalid)
				return (parsingStatus, parsedEnum);
			return EnumVariantThatMatchesMappingRule(type, value);
		}

		private static (EnumParsingStatus, object?) EnumVariantThatMatchesMappingRule(Type type, object? value)
		{
			if (value is not string valueString) return (EnumParsingStatus.ParsingFailedBecauseOneOrMoreTypesAreIncompatible, value);
			var enumMappingRules = type.GetCustomAttributes<StringToEnumVariantMappingRule>();
			if (enumMappingRules is null || enumMappingRules.Count() == 0)
				return (EnumParsingStatus.ParsingFailedBecauseStringIsInvalid, value);
			// Attempt to use one of the mapping rule attributes
			foreach (var enumMappingRule in enumMappingRules)
			{
				if (valueString == enumMappingRule.DatabaseStringValue)
					return (EnumParsingStatus.ParsingSuccessful, enumMappingRule.EnumVariant);
			}
			return (EnumParsingStatus.ParsingFailedBecauseStringIsInvalid, value);
		}

		private static (EnumParsingStatus, object?) AttemptToParseEnum(Type type, object? value)
		{
			if (!type.IsEnum || value is not string valueString)
				return (EnumParsingStatus.ParsingFailedBecauseOneOrMoreTypesAreIncompatible, value);
			var propertyWasParsedEnum = Enum.TryParse(type, valueString, out object? parsedEnumValue);
			if (propertyWasParsedEnum) return (EnumParsingStatus.ParsingSuccessful, parsedEnumValue);
			return (EnumParsingStatus.ParsingFailedBecauseStringIsInvalid, value);
		}
	}
}
