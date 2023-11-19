using Atoms.Utils.Reflection.TypeMapping;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Atoms.Utils.Reflection.TypeMapping.EnumMappingHelpers;

namespace Atoms.Templates.Query.LazySqlServerAsyncEnumerables
{
	internal class ScalarAsyncEnumerable<T> : LazyAsyncEnumerable<T>
	{
		public ScalarAsyncEnumerable(
			string connectionString,
			string sqlText,
			object? parameters,
			Func<Exception, Task>? exceptionHandler,
			CancellationToken cancellationToken
		) : base(connectionString, sqlText, parameters, exceptionHandler, cancellationToken)
		{
		}

		protected override T GetValueFromReader(SqlDataReader reader)
		{
			var scalar = reader[0];
			var (parsingStatus, parsedEnumOrScalar) = AttemptToParseEnumScalar(typeof(T), scalar);
			return (T)parsedEnumOrScalar;
		}
	}
}
