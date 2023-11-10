using Atoms.DataAttributes;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Atoms.Utils.Reflection.AttributeCheckerHelpers;

namespace Atoms.Repositories.SqlServer
{
	internal class SqlTextGenerationUtilities<TModel>
		where TModel : class, new()
	{
		private static readonly IEnumerable<PropertyInfo> modelPublicPropertiesWithUniqueIdAttribute =
			GetPublicPropertiesThatContainAttribute<UniqueIdAttribute>(typeof(TModel));
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
	}
}
