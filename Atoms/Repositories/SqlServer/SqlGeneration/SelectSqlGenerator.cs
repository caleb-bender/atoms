﻿using CalebBender.Atoms.DataAttributes;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static CalebBender.Atoms.Utils.Reflection.TypeMapping.EnumMappingHelpers;

namespace CalebBender.Atoms.Repositories.SqlServer.SqlGeneration
{
	internal class SelectSqlGenerator<TModel>
		where TModel : class, new()
	{
		private static readonly string selectFromText = GetSelectFromSqlText();

		internal static (string, IEnumerable<SqlParameter>) GetSelectSqlTextAndParameters(TModel model, string? entityName)
		{
			var (whereClause, whereParameters) = GetWhereClauseTextAndParametersForSpecificModel(model);
			var selectFromTextWithEntityName = selectFromText;
			if (entityName is not null)
				selectFromTextWithEntityName = selectFromText.Replace($"FROM [{ModelMetadata<TModel>.TableName}]", $"FROM [{entityName}]");
			var selectQuery = selectFromTextWithEntityName + whereClause;
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
				var propertyValue = IfEnumPropertyConvertToDatabaseValueElseUseOriginalValue(property, model);

				sqlParameters.Add(new SqlParameter("@" + propertyName, propertyValue));
				if (i != ModelMetadata<TModel>.UniqueIdPublicProperties.Count() - 1)
					whereClause += " AND ";
			}
			return (whereClause, sqlParameters);
		}

		private static string GetSelectFromSqlText()
		{
			var selectFromText = "SELECT TOP(1) ";
			var modelPublicProperties = ModelMetadata<TModel>.PublicProperties;
			foreach (var modelProperty in modelPublicProperties)
			{
				if (
					PropertyMappingUtilities<TModel>
					.PropertyShouldNotBeReadFromDatabase(modelProperty)
				) continue;
				selectFromText += "[" + ModelMetadata<TModel>.GetDatabasePropertyName(modelProperty) + "], ";
			}
			selectFromText += $"FROM [{ModelMetadata<TModel>.TableName}]";
			selectFromText = selectFromText.Replace(", FROM", " FROM");
			return selectFromText;
		}
	}
}
