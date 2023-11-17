using Atoms.Templates.Mutation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Templates.Query
{
    internal class SqlServerQueryTemplate<T> : IDbQueryTemplate<T>
    {
		internal string ConnectionString { get; init; }
		internal string SqlText { get; init; }
        internal Func<Exception, Task>? ExceptionHandler { get; init; }

		public Task<IEnumerable<T>> QueryAsync(object? parameters = null)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> QueryLazyAsync(object? parameters = null)
        {
            throw new NotImplementedException();
        }
    }
}
