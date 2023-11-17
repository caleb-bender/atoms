using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Templates.Mutation
{
	internal class SqlServerAtomicMutationTemplate : IAtomicMutationTemplate
	{
		internal string ConnectionString { get; init; }
		internal string SqlText { get; init; }
		internal Func<Exception, Task>? ExceptionHandler { get; init; }

		public Task MutateAsync(object? parameters = null)
		{
			throw new NotImplementedException();
		}
	}
}
