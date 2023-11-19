using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Templates.Query.LazySqlServerAsyncEnumerables
{
	internal class ScalarAsyncEnumerable<T> : LazyAsyncEnumerable<T>
	{
		public ScalarAsyncEnumerable(
			string connectionString,
			string sqlText,
			object? parameters
		) : base(connectionString, sqlText, parameters)
		{
		}

		protected override T GetValueFromReader(SqlDataReader reader)
		{
			var scalar = reader[0];
			return (T)scalar;
		}
	}
}
