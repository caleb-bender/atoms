using Atoms.DataAttributes;
using Atoms.Exceptions;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Reflection;
using static Atoms.Utils.Reflection.PropertyInfoRetrieverHelpers;
using static Atoms.Utils.Reflection.TypeMapping.EnumMappingHelpers;

namespace Atoms.Repositories.SqlServer
{
	internal static class PropertyMappingUtilities<TModel>
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
				if (PropertyShouldNotBeReadFromDatabase(modelProperty)) continue;
				var dbPropertyName =
					PropertyMappingUtilities<TModel>.GetDbPropertyNameOfModelProperty(modelProperty);
				var columnValue = reader[dbPropertyName];
				AttemptToMapDatabasePropertyToModelProperty(model, modelProperty, dbPropertyName, columnValue);
			}
			return model;
		}

		private static void AttemptToMapDatabasePropertyToModelProperty(TModel model, PropertyInfo modelProperty, string dbPropertyName, object columnValue)
		{
			try
			{
				AssertDatabaseStringValueDoesNotExceedMaxLength(modelProperty, dbPropertyName, columnValue);
				var columnValueIsNull = columnValue is null || columnValue == DBNull.Value;
				if (columnValueIsNull) modelProperty.SetValue(model, null);
				else modelProperty.SetValue(model, columnValue);
			}
			catch (ArgumentException err)
			{
				var (parsingStatus, enumValue) = AttemptToParseEnumProperty(modelProperty, columnValue);
				if (parsingStatus == EnumParsingStatus.ParsingSuccessful)
					modelProperty.SetValue(model, enumValue);
				else if (parsingStatus == EnumParsingStatus.ParsingFailedBecauseStringIsInvalid)
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

		private static void AssertDatabaseStringValueDoesNotExceedMaxLength(PropertyInfo modelProperty, string dbPropertyName, object columnValue)
		{
			if (columnValue is not string columnValueString) return;
			var maxLengthAttribute = modelProperty.GetCustomAttribute<MaxLengthAttribute>();
			if (maxLengthAttribute is not null && columnValueString.Length > maxLengthAttribute.Length)
				throw new StringPropertyValueExceedsMaxLengthException(
					$"The length of the value of the database property \"{dbPropertyName}\" exceeds the Length of the MaximumLengthAttribute on property \"{typeof(TModel).Name}.{modelProperty.Name}\"."
				);
		}

		private static void AssertModelStringValueDoesNotExceedMaxLength(PropertyInfo modelProperty, TModel model)
		{
			var modelPropertyValue = modelProperty.GetValue(model);
			if (modelPropertyValue is not string stringValue) return;
			var maxLengthAttribute = modelProperty.GetCustomAttribute<MaxLengthAttribute>();
			if (maxLengthAttribute is not null && stringValue.Length > maxLengthAttribute.Length)
				throw new StringPropertyValueExceedsMaxLengthException(
					$"The length of the value of the model property \"{typeof(TModel).Name}.{modelProperty.PropertyType.Name}\" exceeds the Length of its MaximumLengthAttribute."
				);
		}

		private static (object?, bool) DatabasePropertyStringWasDeserializedToObject(PropertyInfo modelProperty, object? columnValue)
		{
			if (PropertyTypeShouldBeSerializedOrDeserialized(modelProperty) && columnValue is string columnValueString)
			{
				object? deserializedObject = null;
				try
				{
					deserializedObject = JsonConvert.DeserializeObject(columnValueString, modelProperty.PropertyType);
				}
				catch (JsonReaderException) { }
				return (deserializedObject, true);
			}
			else
				return (null, false);
		}

		private static bool PropertyTypeShouldBeSerializedOrDeserialized(PropertyInfo modelProperty)
		{
			return modelProperty.PropertyType != typeof(string)
				&& !modelProperty.PropertyType.IsValueType && modelProperty.PropertyType.IsClass;
		}

		internal static bool PropertyShouldNotBeReadFromDatabase(PropertyInfo modelProperty)
		{
			var atomsIgnoreAttribute = modelProperty.GetCustomAttribute<AtomsIgnoreAttribute>();
			var propertyHasAtomsIgnoreAttributeAndReadFromDatabaseIsFalse =
				atomsIgnoreAttribute is not null && atomsIgnoreAttribute.ReadFromDatabase == false;

			return propertyHasAtomsIgnoreAttributeAndReadFromDatabaseIsFalse;
		}

		private static (object?, bool) IfNonStringClassThenSerializeToJson(PropertyInfo modelProperty, TModel model)
		{
			var modelPropertyValue = modelProperty.GetValue(model);
			if (!PropertyTypeShouldBeSerializedOrDeserialized(modelProperty))
				return (modelPropertyValue, false);
			string? serializedJsonString = null;
			var wasSerialized = false;
			try
			{
				serializedJsonString = JsonConvert.SerializeObject(modelPropertyValue);
				wasSerialized = true;
			}
			catch (Exception) { }
			return (serializedJsonString, wasSerialized);
		}

		internal static object? GetModelPropertyDatabaseValue(PropertyInfo modelProperty, TModel model)
		{
			AssertModelStringValueDoesNotExceedMaxLength(modelProperty, model);
			var modelPropertyValue = modelProperty.GetValue(model);

			var (enumToStringMappedValue, wasMappedFromEnumToString) = IfIsEnumThenTryToMapUsingRule(modelProperty, model);
			if (wasMappedFromEnumToString) return enumToStringMappedValue;

			var (enumToString, wasConvertedFromEnumToString) = IfEnumThenConvertToString(modelProperty, model);
			if (wasConvertedFromEnumToString) return enumToString;

			var (serializedJson, wasSerializedToJson) = IfNonStringClassThenSerializeToJson(modelProperty, model);
			if (wasSerializedToJson) return serializedJson;

			return modelPropertyValue;
		}
	}
}
