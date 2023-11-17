using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Templates.Query
{
	public interface IAtomicQueryTemplate<T>
	{
		Task<IEnumerable<T>> QueryAsync(object? parameters = null);
		Task<IEnumerable<T>> QueryLazyAsync(object? parameters = null);
	}
}
