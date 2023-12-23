using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalebBender.Atoms.Exceptions;

namespace CalebBender.Atoms.Templates.Query
{
	/// <summary>
	/// A query template with immutable state used for reusing a constant query
	/// with variable parameters.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IAtomicQueryTemplate<T>
	{
		/// <summary>
		/// Uses the query template defined and the parameters passed in (if defined)
		/// and returns an object used to lazy load the results, only loading one at a time as needed.
		/// Use an "await foreach" loop to iterate over the results.
		/// </summary>
		/// <param name="parameters">
		/// An object with public properties matching the names of the parameters
		/// referenced by the template
		/// </param>
		/// <returns>An IAsyncEnumerable that can be iterated over asynchronously</returns>
		/// <exception cref="DbEntityNotFoundException" />
		/// <exception cref="ModelDbEntityMismatchException" />
		/// <exception cref="ModelPropertyTypeMismatchException" />
		IAsyncEnumerable<T> QueryLazy(object? parameters = null);
	}
}
