﻿using Atoms.DataAttributes;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Repositories.SqlServer.SqlGeneration
{
	internal static class InsertSqlGenerator<TModel>
		where TModel : class, new()
	{
		private static readonly string insertIntoText = GetInsertIntoText();

		internal static (string, IEnumerable<SqlParameter>, PropertyInfo?) GetInsertSqlText(IEnumerable<TModel> models)
		{
			List<List<SqlParameter>> modelsParameters = new List<List<SqlParameter>>();
			PropertyInfo? identityModelProperty = null;
			for (int i = 0; i < models.Count(); i++)
			{
				modelsParameters.Add(new List<SqlParameter>());
				var model = models.ElementAt(i);
				foreach (var modelProperty in ModelMetadata<TModel>.PublicProperties)
				{
					if (PropertyIsAutoGeneratedUniqueId(modelProperty))
					{
						identityModelProperty = modelProperty; continue;
					}
					var databasePropertyName = PropertyMappingUtilities<TModel>.GetDbPropertyNameOfModelProperty(modelProperty);
					modelsParameters.ElementAt(i).Add(new SqlParameter("@" + databasePropertyName + i.ToString(),modelProperty.GetValue(model) ?? DBNull.Value));
				}
			}
			var valuesText = $"VALUES " + GetValuesTuplesToInsert(modelsParameters) + ";";
			var insertSqlText = insertIntoText + valuesText;
			if (identityModelProperty is not null)
			{
				insertSqlText += " SELECT SCOPE_IDENTITY();";
			}
			return (insertSqlText, modelsParameters.SelectMany(selected => selected), identityModelProperty);
		}

		internal static string GetValuesTuplesToInsert(IEnumerable<IEnumerable<SqlParameter>> modelsParameters)
		{
			var valuesTuples = "";
			for (int i = 0; i < modelsParameters.Count(); i++)
			{
				var modelParameters = modelsParameters.ElementAt(i);
				valuesTuples += "(";
				for (int j = 0; j < modelParameters.Count(); j++)
				{
					var modelParameter = modelParameters.ElementAt(j);
					valuesTuples += modelParameter.ParameterName;
					if (j + 1 < modelParameters.Count())
						valuesTuples += ",";
				}
				valuesTuples += ")";
				if (i + 1 < modelsParameters.Count())
					valuesTuples += ",";
			}
			return valuesTuples;
		}

		internal static object GetConvertedIdentityType(PropertyInfo identityModelProperty, object identityValue)
		{
			if (identityModelProperty.PropertyType == typeof(long))
				identityValue = Convert.ToInt64(identityValue);
			else if (identityModelProperty.PropertyType == typeof(int))
				identityValue = Convert.ToInt32(identityValue);
			else if (identityModelProperty.PropertyType == typeof(short))
				identityValue = Convert.ToInt16(identityValue);
			else if (identityModelProperty.PropertyType == typeof(byte))
				identityValue = Convert.ToByte(identityValue);
			return identityValue;
		}

		private static string GetInsertIntoText()
		{
			var insertIntoText = $"INSERT INTO [{ModelMetadata<TModel>.TableName}](";
			int i = 0;
			foreach (var modelProperty in ModelMetadata<TModel>.PublicProperties)
			{
				if (PropertyIsAutoGeneratedUniqueId(modelProperty)) continue;
				var databasePropertyName = PropertyMappingUtilities<TModel>.GetDbPropertyNameOfModelProperty(modelProperty);
				insertIntoText += $"[{databasePropertyName}]";
				if (++i < ModelMetadata<TModel>.PublicProperties.Count())
					insertIntoText += ", ";
			}
			insertIntoText += ") ";
			insertIntoText = insertIntoText.Replace(", )", ")");
			return insertIntoText;
		}

		private static bool PropertyIsAutoGeneratedUniqueId(PropertyInfo modelProperty)
		{
			return modelProperty.GetCustomAttribute<UniqueIdAttribute>() is UniqueIdAttribute uniqueIdAttribute && uniqueIdAttribute.AutoGenerated;
		}
	}
}
