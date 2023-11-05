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
	internal static class PropertyMappingUtilities<TModel>
		where TModel : class, new()
	{

		private static readonly IEnumerable<PropertyInfo> modelPublicPropertiesWithUniqueIdAttribute =
			GetPublicPropertiesThatContainAttribute<UniqueIdAttribute>(typeof(TModel));

		private static readonly PropertyInfo namePropertyOfDbPropertyNameAttribute =
			GetAllPublicProperties(typeof(DbPropertyNameAttribute)).First();

		internal static (string, IEnumerable<SqlParameter>) GetSelectSqlTextAndParameters(TModel model)
		{
			var tableName = typeof(TModel).Name + "s";
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
				whereClause += $"{property.Name} = @{property.Name}";
				sqlParameters.Add(new SqlParameter(property.Name, property.GetValue(model)));
				if (i != modelPublicPropertiesWithUniqueIdAttribute.Count() - 1)
					whereClause += " AND ";
			}
			return (whereClause, sqlParameters);
		}

		internal static string GetDbPropertyNameOfModelProperty(PropertyInfo modelProperty)
		{
			var dbPropertyNameAttribute =
				modelProperty.GetCustomAttribute(typeof(DbPropertyNameAttribute));
			if (dbPropertyNameAttribute is null) return modelProperty.Name;
			string propertyName = (string?)namePropertyOfDbPropertyNameAttribute.GetValue(dbPropertyNameAttribute) ?? "";
			return propertyName;
		}
	}
}
