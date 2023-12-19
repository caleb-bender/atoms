using CalebBender.Atoms.DataAttributes;
using CalebBender.Atoms.Repositories.SqlServer;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CalebBender.Atoms.Utils.Reflection.TypeMapping
{
	internal static class EnumMappingHelpers
	{
		internal enum EnumParsingStatus
		{
			ParsingSuccessful,
			ParsingFailedBecauseStringIsInvalid,
			ParsingFailedBecauseOneOrMoreTypesAreIncompatible
		}

		internal static object? IfEnumPropertyConvertToDatabaseValueElseUseOriginalValue(PropertyInfo modelProperty, object? model)
		{
			var (enumToStringMappedValue, wasMappedFromEnumToString) = IfIsEnumThenTryToMapUsingRule(modelProperty, model);
			if (wasMappedFromEnumToString) return enumToStringMappedValue;

			var (enumToString, wasConvertedFromEnumToString) = IfEnumThenConvertToString(modelProperty, model);
			if (wasConvertedFromEnumToString) return enumToString;

			return modelProperty.GetValue(model);
		}

		internal static (EnumParsingStatus, object?) AttemptToParseEnumScalar(Type type, object? scalar)
		{
			return EnumParsingStatusAndParsedValue(type, scalar);
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

		internal static (object?, bool) IfIsEnumThenTryToMapUsingRule(PropertyInfo modelProperty, object? model)
		{
			object? modelPropertyValue = modelProperty.GetValue(model);
			if (!modelProperty.PropertyType.IsEnum || modelPropertyValue is null)
				return (modelPropertyValue, false);
			var stringToEnumMappings = modelProperty.PropertyType.GetCustomAttributes<StringToEnumVariantMappingRule>();
			foreach (var mappingRule in stringToEnumMappings)
			{
				int enumVariantInt = Convert.ToInt32(mappingRule.EnumVariant);
				int modelPropertyValueInt = Convert.ToInt32(modelPropertyValue);
				if (enumVariantInt == modelPropertyValueInt)
					return (mappingRule.DatabaseStringValue, true);
			}
			return (modelPropertyValue, false);
		}

		internal static (object?, bool) IfEnumThenConvertToString(PropertyInfo modelProperty, object? model)
		{
			if (modelProperty.PropertyType.IsEnum)
				return (modelProperty.GetValue(model)?.ToString(), true);
			return (null, false);
		}

		internal static object IfEnumConvertToStringElseReturnOriginalValue(object value)
		{
			if (!value.GetType().IsEnum) return value;
			var stringToEnumMappings = value.GetType().GetCustomAttributes<StringToEnumVariantMappingRule>();
			foreach (var mappingRule in stringToEnumMappings)
			{
				int enumVariantInt = Convert.ToInt32(mappingRule.EnumVariant);
				int valueInt = Convert.ToInt32(value);
				if (enumVariantInt == valueInt)
					return mappingRule.DatabaseStringValue;
			}
			return value.ToString() ?? "";
		}
	}
}
