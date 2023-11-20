using CalebBender.Atoms.Exceptions;
using CalebBender.Atoms.Templates.Mutation;
using CalebBender.Atoms.Templates.Query.LazySqlServerAsyncEnumerables;
using CalebBender.Atoms.Utils.Reflection.Tuples;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CalebBender.Atoms.Templates.Query
{
    internal class SqlServerAtomicQueryTemplate<T> : IAtomicQueryTemplate<T>
    {
		internal CancellationToken CancellationToken { get; init; }
		internal string ConnectionString { get; init; }
		internal string SqlText { get; init; }
        internal Func<Exception, Task>? ExceptionHandler { get; init; }

        public IAsyncEnumerable<T> QueryLazy(object? parameters = null)
        {
            return new LazyAsyncEnumerableFactory<T>().Create(
                ConnectionString, SqlText, parameters, ExceptionHandler, CancellationToken
            );
        }
	}
}
