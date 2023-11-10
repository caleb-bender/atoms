using Atoms.DataAttributes;
using Atoms.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Atoms.Utils.Reflection.PropertyInfoRetrieverHelpers;

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
				var dbPropertyName =
					PropertyMappingUtilities<TModel>.GetDbPropertyNameOfModelProperty(modelProperty);
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
				modelProperty.SetValue(model, columnValue);
			}
			catch (ArgumentException err)
			{
				var (parsingStatus, enumValue) = AttemptToParseEnumProperty(modelProperty, columnValue);
				if (parsingStatus == EnumPropertyParsingStatus.ParsingSuccessful)
					modelProperty.SetValue(model, enumValue);
				else if (parsingStatus == EnumPropertyParsingStatus.ParsingFailedBecauseStringIsInvalid)
					throw new EnumPropertyMappingFailedException($"The database property \"{dbPropertyName}\" with value = \"{columnValue}\" could not be mapped to the enum property \"{typeof(TModel).Name}.{modelProperty.Name}\".");
				else
					throw new ModelPropertyTypeMismatchException($"The type of the database property \"{dbPropertyName}\" could not be converted to the type of the \"{typeof(TModel).Name}.{modelProperty.Name}\" property.", err);
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
			return (EnumPropertyParsingStatus.ParsingFailedBecauseStringIsInvalid, columnValue);
		}
	}
}
