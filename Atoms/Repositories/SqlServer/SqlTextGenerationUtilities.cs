using Atoms.DataAttributes;
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
	internal class SqlTextGenerationUtilities<TModel>
		where TModel : class, new()
	{
		private static readonly IEnumerable<PropertyInfo> modelPublicProperties = GetAllPublicProperties(typeof(TModel));
		private static readonly IEnumerable<PropertyInfo> modelPublicPropertiesWithUniqueIdAttribute =
			GetPublicPropertiesThatContainAttribute<UniqueIdAttribute>(typeof(TModel));
		private static readonly Type modelType = typeof(TModel);
		private static readonly DbEntityNameAttribute? dbEntityNameAttribute = modelType.GetCustomAttribute<DbEntityNameAttribute>();
		private static readonly string databaseTableName = GetDatabaseTableName();

		internal static (string, IEnumerable<SqlParameter>) GetSelectSqlTextAndParameters(TModel model)
		{
			var selectQuery = $"SELECT TOP(1) * FROM [{databaseTableName}]";
			var (whereClause, whereParameters) = GetWhereClauseTextAndParametersForSpecificModel(model);
			selectQuery += whereClause;
			return (selectQuery, whereParameters);
		}

		internal static string GetDatabaseTableName()
		{
			var tableName = modelType.Name + "s";
			if (dbEntityNameAttribute is not null)
				tableName = dbEntityNameAttribute.Name;
			return tableName;
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

		internal static (string, IEnumerable<SqlParameter>) GetInsertSqlTextForOne(TModel model)
		{
			var insertIntoText = $"INSERT INTO [{databaseTableName}](";
			int i = 0;
			List<SqlParameter> parameters = new List<SqlParameter>();
			foreach (var modelProperty in modelPublicProperties)
			{
				if (modelProperty.GetValue(model) is null) continue;
				var databasePropertyName = PropertyMappingUtilities<TModel>.GetDbPropertyNameOfModelProperty(modelProperty);
				insertIntoText += $"[{databasePropertyName}]";
				if (++i < modelPublicProperties.Count())
					insertIntoText += ", ";
				parameters.Add(new SqlParameter("@" + databasePropertyName, modelProperty.GetValue(model)));
			}
			insertIntoText += ") ";
			insertIntoText = insertIntoText.Replace(", )", ")");
			var valuesText = $"VALUES ({string.Join(',', parameters.Select(parameter => parameter.ParameterName))});";

			return (insertIntoText + valuesText, parameters);
		}

		private static string GetDatabasePropertyName(PropertyInfo property)
		{
			var propertyName = property.Name;
			var dbPropertyNameAttribute = property.GetCustomAttribute<DbPropertyNameAttribute>();
			if (dbPropertyNameAttribute is not null) propertyName = dbPropertyNameAttribute.Name;
			return propertyName;
		}
	}
}
