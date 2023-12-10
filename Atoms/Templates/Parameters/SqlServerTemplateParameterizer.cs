using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static CalebBender.Atoms.Utils.Reflection.PropertyInfoRetrieverHelpers;
using static CalebBender.Atoms.Utils.Reflection.TypeMapping.EnumMappingHelpers;
using static CalebBender.Atoms.Utils.Reflection.TypeCheckingHelpers;
using System.Collections;

namespace CalebBender.Atoms.Templates.Parameters
{
	internal class SqlServerTemplateParameterizer
	{
		private readonly IEnumerable<PropertyInfo>? parameterObjectProperties;
		private readonly string sqlText;
		private readonly object? parameters;

		public SqlServerTemplateParameterizer(string sqlText, object? parameters = null)
		{
			this.sqlText = sqlText;
			this.parameters = parameters;
			if (parameters is not null)
				parameterObjectProperties = GetAllPublicProperties(parameters.GetType());
		}

		internal string AddParametersAndGetExpandedSqlText(SqlCommand command)
		{
			if (parameterObjectProperties is null) return sqlText;
			string expandedSqlText = sqlText;
			foreach (var property in parameterObjectProperties)
			{
				var propertyValue =
					IfEnumPropertyConvertToDatabaseValueElseUseOriginalValue(property, parameters);
				expandedSqlText = AddEachParameterAndGetExpandedSqlText(command, property, propertyValue);	
			}
			return expandedSqlText;
		}


		private string AddEachParameterAndGetExpandedSqlText(SqlCommand command, PropertyInfo property, object? propertyValue)
		{
			var expandedSqlText = sqlText;
			var (isEnumerable, enumerable) = IsIEnumerable(propertyValue);
			if (!isEnumerable)
			{
				command.Parameters.AddWithValue("@" + property.Name, propertyValue);
				return expandedSqlText;
			}
			var enumerableOfValues = enumerable.Cast<object>();
			expandedSqlText = GetSqlTextWithExpandedIEnumerableParameters(enumerableOfValues);
			int i = 0;
			foreach (var value in enumerableOfValues)
			{
				var convertedValue = IfEnumConvertToStringElseReturnOriginalValue(value);
				command.Parameters.AddWithValue("@" + property.Name + i, convertedValue);
				i++;
			}
			return expandedSqlText;
		}
		private string GetSqlTextWithExpandedIEnumerableParameters(IEnumerable<object> enumerableOfValues)
		{
			var regex = new Regex(" (I|i)(N|n) (@\\w+)");
			var matches = regex.Matches(sqlText);
			var expandedSqlText = sqlText;
			foreach (Match match in matches)
			{
				var enumerableParameterName = match.Groups[3].Value;
				int i = 0;
				var expandedParameterNames = enumerableOfValues.Select(v => enumerableParameterName + i++).ToArray();
				string expandedParameters = "(" + string.Join(',', expandedParameterNames) + ")";
				expandedSqlText = expandedSqlText.Replace(enumerableParameterName, expandedParameters);
			}
			return expandedSqlText;
		}
	}
}
