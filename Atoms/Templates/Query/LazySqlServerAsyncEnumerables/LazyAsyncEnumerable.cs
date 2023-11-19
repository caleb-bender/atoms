using Atoms.Exceptions;
using Atoms.Utils.Reflection.Tuples;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Atoms.Utils.Reflection.PropertyInfoRetrieverHelpers;
using static Atoms.Utils.Reflection.TypeMapping.EnumMappingHelpers;

namespace Atoms.Templates.Query.LazySqlServerAsyncEnumerables
{
	internal abstract class LazyAsyncEnumerable<T> : IAsyncEnumerable<T>
	{
		private string connectionString;
		private string sqlText;
		private object? parameters;
		private IEnumerable<PropertyInfo>? parameterObjectProperties;

		protected LazyAsyncEnumerable(string connectionString, string sqlText, object? parameters)
		{
			this.connectionString = connectionString;
			this.sqlText = sqlText;
			this.parameters = parameters;
			if (parameters is not null)
			{
				parameterObjectProperties = GetAllPublicProperties(parameters.GetType());
			}
		}

		public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
		{
			using SqlConnection connection = new SqlConnection(connectionString);
			connection.Open();
			using SqlCommand queryCommand = new SqlCommand(sqlText, connection);
			AddParametersIfTheyExist(queryCommand);
			using var reader = await queryCommand.ExecuteReaderAsync();
			while (await reader.ReadAsync())
			{
				T value = default(T);
				try
				{
					value = GetValueFromReader(reader);
				}
				catch (InvalidCastException) { }
				yield return value;
			}
		}

		private void AddParametersIfTheyExist(SqlCommand queryCommand)
		{
			if (parameterObjectProperties is null) return;
			foreach (var property in parameterObjectProperties)
			{
				var propertyValue =
					IfEnumPropertyConvertToDatabaseValueElseUseOriginalValue(property, parameters);
				queryCommand.Parameters.AddWithValue("@" + property.Name, propertyValue);
			}
		}

		protected abstract T GetValueFromReader(SqlDataReader reader);
	}
}
