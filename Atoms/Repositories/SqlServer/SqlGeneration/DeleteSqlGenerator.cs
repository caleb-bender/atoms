﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CalebBender.Atoms.Repositories.SqlServer.SqlGeneration
{
	internal static class DeleteSqlGenerator<TModel>
		where TModel : class, new()
	{
		internal static (string, IEnumerable<SqlParameter>) GetDeleteTextAndParameters(IEnumerable<TModel> models, string? entityName)
		{
			var deleteWhereClause = " WHERE ";
			var deleteParameters = new List<SqlParameter>();
			var uniqueIdProperties = ModelMetadata<TModel>.UniqueIdPublicProperties;
			for (int i = 0; i < uniqueIdProperties.Count(); i++)
			{
				var uniqueIdProperty = uniqueIdProperties.ElementAt(i);
				var uniqueIdName = ModelMetadata<TModel>.GetDatabasePropertyName(uniqueIdProperty);
				var (commaSeparatedValues, parameters) =
					GetCommaSeparatedValuesWithParameters(models, uniqueIdName, uniqueIdProperty);
				deleteWhereClause +=  "[" + uniqueIdName + "] IN (" + commaSeparatedValues + ")";
				deleteParameters.AddRange(parameters);
				if (i + 1 < uniqueIdProperties.Count())
					deleteWhereClause += " AND ";
			}
			var deleteFromText = GetDeleteFromText(entityName);
			return (deleteFromText + deleteWhereClause, deleteParameters);
		}

		private static (string, IEnumerable<SqlParameter>) GetCommaSeparatedValuesWithParameters(
			IEnumerable<TModel> models, string uniqueIdName, PropertyInfo uniqueIdProperty
		)
		{
			var commaSeparatedValues = "";
			var tupleParameters = new List<SqlParameter>();
			for (int j = 0; j < models.Count(); j++)
			{
				var model = models.ElementAt(j);
				if (model is null) throw new ArgumentNullException("A model passed to DeleteManyAsync cannot be null.");
				var parameterName = "@" + uniqueIdName + j;
				var parameterValue = PropertyMappingUtilities<TModel>.GetModelPropertyDatabaseValue(uniqueIdProperty, model);
				tupleParameters.Add(new SqlParameter(parameterName, parameterValue));
				commaSeparatedValues += parameterName;
				if (j + 1 < models.Count())
					commaSeparatedValues += ",";
			}
			return (commaSeparatedValues, tupleParameters);
		}

		private static string GetDeleteFromText(string? entityName)
		{
			var deleteFromText = "DELETE FROM [" + (entityName ?? ModelMetadata<TModel>.TableName) + "]";
			return deleteFromText;
		}

		
	}
}
