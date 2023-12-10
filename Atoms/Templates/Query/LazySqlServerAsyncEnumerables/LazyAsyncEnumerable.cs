using CalebBender.Atoms.Exceptions;
using CalebBender.Atoms.Templates.Parameters;
using CalebBender.Atoms.Utils.Reflection.Tuples;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
				using SqlCommand queryCommand = connection.CreateCommand();
				var parameterizer = new SqlServerTemplateParameterizer(sqlText, parameters);
				var expandedSqlText = parameterizer.AddParametersAndGetExpandedSqlText(queryCommand);
				queryCommand.CommandText = expandedSqlText;
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

		protected abstract T GetValueFromReader(SqlDataReader reader);
	}
}
