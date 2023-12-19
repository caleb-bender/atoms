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
				expandedSqlText = AddEachParameterAndGetExpandedSqlText(command, property, propertyValue, expandedSqlText);	
			}
			return expandedSqlText;
		}


		private string AddEachParameterAndGetExpandedSqlText(SqlCommand command, PropertyInfo property, object? propertyValue, string expandedSqlText)
		{
			if (propertyValue is null) throw new ArgumentNullException("A parameter used within an anonymous object passed to a template cannot be null.");
			var (isEnumerable, enumerable) = IsIEnumerable(propertyValue);
			var propertyName = "@" + property.Name;
			if (!isEnumerable)
			{
				command.Parameters.AddWithValue(propertyName, propertyValue);
				return expandedSqlText;
			}
			var enumerableOfValues = enumerable.Cast<object>();
			expandedSqlText = GetSqlTextWithExpandedIEnumerableParameters(propertyName, enumerableOfValues, expandedSqlText);
			int i = 0;
			foreach (var value in enumerableOfValues)
			{
				object? convertedValue = null;
				if (value is not null)
					convertedValue = IfEnumConvertToStringElseReturnOriginalValue(value);
				if (convertedValue is null) throw new ArgumentNullException("An IEnumerable parameter must contain all non-null values.");
				command.Parameters.AddWithValue(propertyName + i, convertedValue);
				i++;
			}
			return expandedSqlText;
		}
		private string GetSqlTextWithExpandedIEnumerableParameters(string propertyName, IEnumerable<object> enumerableOfValues, string expandedSqlText)
		{
			var regex = new Regex(" (I|i)(N|n) (@\\w+)");
			var matches = regex.Matches(sqlText);
			foreach (Match match in matches)
			{
				var enumerableParameterName = match.Groups[3].Value;
				if (enumerableParameterName != propertyName) continue;
				int i = 0;
				var expandedParameterNames = enumerableOfValues.Select(v => enumerableParameterName + i++).ToArray();
				string expandedParameters = "(" + string.Join(',', expandedParameterNames) + ")";
				expandedSqlText = expandedSqlText.Replace(enumerableParameterName, expandedParameters);
			}
			return expandedSqlText;
		}
	}
}
