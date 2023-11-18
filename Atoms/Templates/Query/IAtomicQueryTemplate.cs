using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Templates.Query
{
	public interface IAtomicQueryTemplate<T>
	{
		IAsyncEnumerable<T> QueryLazy(object? parameters = null);
	}
}
