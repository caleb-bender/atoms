using Atoms.DataAttributes;
using Atoms.Exceptions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Atoms.Utils.Reflection.AttributeCheckerHelpers;
using static Atoms.Utils.Reflection.PropertyInfoRetrieverHelpers;

namespace Atoms.Repositories.SqlServer
{
	internal static class PropertyMappingUtilities<TModel>
		where TModel : class, new()
	{

		private static readonly IEnumerable<PropertyInfo> modelPublicProperties = GetAllPublicProperties(typeof(TModel));

		private static readonly IEnumerable<PropertyInfo> modelPublicPropertiesWithUniqueIdAttribute =
			GetPublicPropertiesThatContainAttribute<UniqueIdAttribute>(typeof(TModel));

		private static readonly PropertyInfo namePropertyOfDbPropertyNameAttribute =
			GetAllPublicProperties(typeof(DbPropertyNameAttribute)).First();

		private static readonly Type modelType = typeof(TModel);
		private static readonly DbEntityNameAttribute? dbEntityNameAttribute = modelType.GetCustomAttribute<DbEntityNameAttribute>();

		internal static (string, IEnumerable<SqlParameter>) GetSelectSqlTextAndParameters(TModel model)
		{
			var tableName = modelType.Name + "s";
			if (dbEntityNameAttribute is not null)
				tableName = dbEntityNameAttribute.Name;
			var selectQuery = $"SELECT TOP(1) * FROM [{tableName}]";
			var (whereClause, whereParameters) = GetWhereClauseTextAndParametersForSpecificModel(model);
			selectQuery += whereClause;
			return (selectQuery, whereParameters);
		}

		internal static (string, IEnumerable<SqlParameter>) GetWhereClauseTextAndParametersForSpecificModel(TModel model)
		{
			List<SqlParameter> sqlParameters = new List<SqlParameter>();
			var whereClause = " WHERE ";
			for (int i = 0; i < modelPublicPropertiesWithUniqueIdAttribute.Count(); i++)
			{
				var property = modelPublicPropertiesWithUniqueIdAttribute.ElementAt(i);
				string propertyName = GetDatabasePropertyName(property);
				whereClause += $"{propertyName} = @{propertyName}";
				var propertyValue = property.GetValue(model);
				if (property.PropertyType.IsEnum) propertyValue = propertyValue?.ToString();
				sqlParameters.Add(new SqlParameter("@" + propertyName, propertyValue));
				if (i != modelPublicPropertiesWithUniqueIdAttribute.Count() - 1)
					whereClause += " AND ";
			}
			return (whereClause, sqlParameters);
		}

		private static string GetDatabasePropertyName(PropertyInfo property)
		{
			var propertyName = property.Name;
			var dbPropertyNameAttribute = property.GetCustomAttribute<DbPropertyNameAttribute>();
			if (dbPropertyNameAttribute is not null) propertyName = dbPropertyNameAttribute.Name;
			return propertyName;
		}

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
