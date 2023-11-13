using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Repositories.SqlServer.SqlGeneration
{
	internal class SelectSqlGenerator<TModel>
		where TModel : class, new()
	{
		internal static (string, IEnumerable<SqlParameter>) GetSelectSqlTextAndParameters(TModel model)
		{
			var selectQuery = $"SELECT TOP(1) * FROM [{ModelMetadata<TModel>.TableName}]";
			var (whereClause, whereParameters) = GetWhereClauseTextAndParametersForSpecificModel(model);
			selectQuery += whereClause;
			return (selectQuery, whereParameters);
		}

		internal static (string, IEnumerable<SqlParameter>) GetWhereClauseTextAndParametersForSpecificModel(TModel model)
		{
			List<SqlParameter> sqlParameters = new List<SqlParameter>();
			var whereClause = " WHERE ";
			for (int i = 0; i < ModelMetadata<TModel>.UniqueIdPublicProperties.Count(); i++)
			{
				var property = ModelMetadata<TModel>.UniqueIdPublicProperties.ElementAt(i);
				string propertyName = ModelMetadata<TModel>.GetDatabasePropertyName(property);
				whereClause += $"{propertyName} = @{propertyName}";
				var propertyValue = property.GetValue(model);
				if (property.PropertyType.IsEnum) propertyValue = propertyValue?.ToString();
				sqlParameters.Add(new SqlParameter("@" + propertyName, propertyValue));
				if (i != ModelMetadata<TModel>.UniqueIdPublicProperties.Count() - 1)
					whereClause += " AND ";
			}
			return (whereClause, sqlParameters);
		}
	}
}
