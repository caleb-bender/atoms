using Atoms.Exceptions;
using Atoms.Utils.Reflection.Tuples;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Templates.Query.LazySqlServerAsyncEnumerables
{
	internal abstract class LazyAsyncEnumerable<T> : IAsyncEnumerable<T>
	{
		private string connectionString;
		private string sqlText;
		private object? parameters;

		protected LazyAsyncEnumerable(string connectionString, string sqlText, object? parameters)
		{
			this.connectionString = connectionString;
			this.sqlText = sqlText;
			this.parameters = parameters;
		}

		public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
		{
			using SqlConnection connection = new SqlConnection(connectionString);
			connection.Open();
			using SqlCommand queryCommand = new SqlCommand(sqlText, connection);
			using var reader = await queryCommand.ExecuteReaderAsync();
			while (await reader.ReadAsync())
			{
				yield return GetValueFromReader(reader);
			}
		}

		protected abstract T GetValueFromReader(SqlDataReader reader);
	}
}
