﻿using Atoms.DataAttributes;
using Atoms.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static Atoms.Utils.Reflection.PropertyInfoRetrieverHelpers;

namespace Atoms.Repositories.SqlServer
{
	internal static class InboundPropertyMappingUtilities<TModel>
		where TModel : class, new()
	{

		private static readonly IEnumerable<PropertyInfo> modelPublicProperties = GetAllPublicProperties(typeof(TModel));


		private static readonly PropertyInfo namePropertyOfDbPropertyNameAttribute =
			GetAllPublicProperties(typeof(DbPropertyNameAttribute)).First();

		internal static string GetDbPropertyNameOfModelProperty(PropertyInfo modelProperty)
		{
			var dbPropertyNameAttribute =
				modelProperty.GetCustomAttribute(typeof(DbPropertyNameAttribute));
			if (dbPropertyNameAttribute is null) return modelProperty.Name;
			string propertyName = (string?)namePropertyOfDbPropertyNameAttribute.GetValue(dbPropertyNameAttribute) ?? "";
			return propertyName;
		}

		internal static TModel GetModelWithMappedProperties(SqlDataReader reader)
		{
			var model = new TModel();
			foreach (var modelProperty in modelPublicProperties)
			{
				var dbPropertyName =
					InboundPropertyMappingUtilities<TModel>.GetDbPropertyNameOfModelProperty(modelProperty);
				var columnValue = reader[dbPropertyName];
				AttemptToMapDatabasePropertyToModelProperty(model, modelProperty, dbPropertyName, columnValue);
			}
			return model;
		}

		private enum EnumPropertyParsingStatus
		{
			ParsingSuccessful,
			ParsingFailedBecauseStringIsInvalid,
			ParsingFailedBecauseOneOrMoreTypesAreIncompatible
		}

		private static void AttemptToMapDatabasePropertyToModelProperty(TModel model, PropertyInfo modelProperty, string dbPropertyName, object columnValue)
		{
			try
			{
				AssertStringValueDoesNotExceedMaxLength(modelProperty, dbPropertyName, columnValue);
				var columnValueIsNull = columnValue is null || columnValue == DBNull.Value;
				if (columnValueIsNull) modelProperty.SetValue(model, null);
				else modelProperty.SetValue(model, columnValue);
			}
			catch (ArgumentException err)
			{
				var (parsingStatus, enumValue) = AttemptToParseEnumProperty(modelProperty, columnValue);
				if (parsingStatus == EnumPropertyParsingStatus.ParsingSuccessful)
					modelProperty.SetValue(model, enumValue);
				else if (parsingStatus == EnumPropertyParsingStatus.ParsingFailedBecauseStringIsInvalid)
					throw new EnumPropertyMappingFailedException($"The database property \"{dbPropertyName}\" with value = \"{columnValue}\" could not be mapped to the enum property \"{typeof(TModel).Name}.{modelProperty.Name}\".");
				else
				{
					var (deserializedObject, wasDeserialized) = DatabasePropertyStringWasDeserializedToObject(modelProperty, columnValue);
					if (!wasDeserialized)
						throw new ModelPropertyTypeMismatchException($"The type of the database property \"{dbPropertyName}\" could not be converted to the type of the \"{typeof(TModel).Name}.{modelProperty.Name}\" property.", err);
					modelProperty.SetValue(model, deserializedObject);
				}
			}
		}

		private static void AssertStringValueDoesNotExceedMaxLength(PropertyInfo modelProperty, string dbPropertyName, object columnValue)
		{
			if (columnValue is not string columnValueString) return;
			var maxLengthAttribute = modelProperty.GetCustomAttribute<MaxLengthAttribute>();
			if (maxLengthAttribute is not null && columnValueString.Length > maxLengthAttribute.Length)
				throw new PropertyValueExceedsMaxLengthException(
					$"The length of the value of the database property \"{dbPropertyName}\" exceeds the Length of the MaximumLengthAttribute on property \"{typeof(TModel).Name}.{modelProperty.Name}\"."
				);
		}

		private static (EnumPropertyParsingStatus, object?) AttemptToParseEnumProperty(PropertyInfo modelProperty, object? columnValue)
		{
			var modelPropertyType = modelProperty.PropertyType;
			if (!modelPropertyType.IsEnum || columnValue is not string columnValueString)
				return (EnumPropertyParsingStatus.ParsingFailedBecauseOneOrMoreTypesAreIncompatible, columnValue);
			var propertyWasParsedEnum = Enum.TryParse(modelPropertyType, columnValueString, out object? parsedEnumValue);
			if (propertyWasParsedEnum) return (EnumPropertyParsingStatus.ParsingSuccessful, parsedEnumValue);
			return EnumVariantThatMatchesMappingRule(modelProperty, columnValue, columnValueString);
		}

		private static (InboundPropertyMappingUtilities<TModel>.EnumPropertyParsingStatus, object?) EnumVariantThatMatchesMappingRule(PropertyInfo modelProperty, object? columnValue, string columnValueString)
		{
			var enumMappingRules = modelProperty.PropertyType.GetCustomAttributes<StringToEnumVariantMappingRule>();
			if (enumMappingRules is null || enumMappingRules.Count() == 0)
				return (EnumPropertyParsingStatus.ParsingFailedBecauseStringIsInvalid, columnValue);
			// Attempt to use one of the mapping rule attributes
			foreach (var enumMappingRule in enumMappingRules)
			{
				if (columnValueString == enumMappingRule.DatabasePropertyValue)
					return (EnumPropertyParsingStatus.ParsingSuccessful, enumMappingRule.DesiredEnumVariant);
			}
			return (EnumPropertyParsingStatus.ParsingFailedBecauseStringIsInvalid, columnValue);
		}

		private static (object?, bool) DatabasePropertyStringWasDeserializedToObject(PropertyInfo modelProperty, object? columnValue)
		{
			var theModelPropertyTypeIsAClassThatIsNotAString = modelProperty.PropertyType != typeof(string)
						&& !modelProperty.PropertyType.IsValueType && modelProperty.PropertyType.IsClass;
			if (theModelPropertyTypeIsAClassThatIsNotAString && columnValue is string columnValueString)
			{
				object? deserializedObject = null;
				try
				{
					deserializedObject = JsonConvert.DeserializeObject(columnValueString, modelProperty.PropertyType);
				}
				catch (JsonReaderException) {}
				return (deserializedObject, true);
			}
			else
				return (null, false);
		}
	}
}