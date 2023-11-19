using Atoms.Utils.Reflection.Tuples;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Templates.Query.LazySqlServerAsyncEnumerables
{
	internal class LazyAsyncEnumerableFactory<T>
	{
		public LazyAsyncEnumerable<T> Create(
			string connectionString, string sqlText, object? parameters, Func<Exception, Task>? exceptionHandler, CancellationToken cancellationToken
		)
		{
			if (ValueTupleHelpers<T>.IsValueTuple)
				return new ValueTupleAsyncEnumerable<T>(connectionString, sqlText, parameters, exceptionHandler, cancellationToken);
			else
				return new ScalarAsyncEnumerable<T>(connectionString, sqlText, parameters, exceptionHandler, cancellationToken);
		}
	}
}
