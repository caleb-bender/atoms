using CalebBender.Atoms.Exceptions;
using CalebBender.Atoms.Utils.Reflection.Tuples;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static CalebBender.Atoms.Utils.Reflection.PropertyInfoRetrieverHelpers;
using static CalebBender.Atoms.Utils.Reflection.TypeMapping.EnumMappingHelpers;
using static CalebBender.Atoms.Repositories.SqlServer.SqlServerErrorTranslators;

namespace CalebBender.Atoms.Templates.Query.LazySqlServerAsyncEnumerables
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
			var (connection, reader) = await GetConnectionAndDataReader();
			try
			{
				while (await reader.ReadAsync(cancellationToken))
				{
					T value = default(T);
					try
					{
						value = GetValueFromReader(reader);
					}
					catch (Exception err)
					{
						if (ShouldHandleException(err))
						{
							if (exceptionHandler is not null)
								await exceptionHandler.Invoke(err);
							else
								throw;
						}
					}
					yield return value;
				}
			}
			finally
			{
				connection.Dispose();
				reader.Dispose();
			}
			
		}

		private async Task<(SqlConnection, SqlDataReader)> GetConnectionAndDataReader()
		{
			var connection = new SqlConnection(connectionString);
			try
			{
				connection.Open();
				using SqlCommand queryCommand = new SqlCommand(sqlText, connection);
				AddParametersIfTheyExist(queryCommand);
				var reader = await queryCommand.ExecuteReaderAsync(cancellationToken);
				return (connection, reader);
			}
			catch (SqlException sqlException)
			{
				connection.Dispose();
				TranslateInvalidObjectNameError(sqlException, typeof(T));
				throw;
			}
		}

		private bool ShouldHandleException(Exception err)
		{
			return err is not InvalidCastException invalidCastException
				|| !invalidCastException.Message.Contains("DBNull");
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
