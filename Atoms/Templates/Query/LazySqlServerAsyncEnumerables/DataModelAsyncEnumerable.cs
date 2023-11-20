using Atoms.Repositories.SqlServer;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Templates.Query.LazySqlServerAsyncEnumerables
{
	internal class DataModelAsyncEnumerable<T> : LazyAsyncEnumerable<T>
	{
		public DataModelAsyncEnumerable(
			string connectionString, string sqlText, object? parameters,
			Func<Exception, Task>? exceptionHandler,
			CancellationToken cancellationToken
		) : base(connectionString, sqlText, parameters, exceptionHandler, cancellationToken)
		{
		}

		protected override T GetValueFromReader(SqlDataReader reader)
		{
			var model = PropertyMappingUtilities<T>.GetModelWithMappedProperties(reader);
			return model;
		}
	}
}
