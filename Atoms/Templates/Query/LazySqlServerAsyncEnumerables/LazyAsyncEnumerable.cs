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
		private readonly Func<Exception, Task>? exceptionHandler;
		private readonly CancellationToken cancellationToken;
		private IEnumerable<PropertyInfo>? parameterObjectProperties;

		protected LazyAsyncEnumerable(
			string connectionString, string sqlText, object? parameters,
			Func<Exception, Task>? exceptionHandler, CancellationToken cancellationToken
		)
		{
			this.connectionString = connectionString;
			this.sqlText = sqlText;
			this.parameters = parameters;
			this.exceptionHandler = exceptionHandler;
			this.cancellationToken = cancellationToken;
			if (parameters is not null)
				parameterObjectProperties = GetAllPublicProperties(parameters.GetType());
		}

		public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken localCancellationToken = default)
		{
			localCancellationToken = cancellationToken;
			using SqlConnection connection = new SqlConnection(connectionString);
			connection.Open();
			using SqlCommand queryCommand = new SqlCommand(sqlText, connection);
			AddParametersIfTheyExist(queryCommand);
			using var reader = await queryCommand.ExecuteReaderAsync(localCancellationToken);
			while (await reader.ReadAsync(localCancellationToken))
			{
				T value = default(T);
				try
				{
					value = GetValueFromReader(reader);
				}
				catch (Exception err) {
					if (exceptionHandler is not null)
						await exceptionHandler.Invoke(err);
				}
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
