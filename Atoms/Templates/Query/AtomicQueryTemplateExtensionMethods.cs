using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalebBender.Atoms.Exceptions;

namespace CalebBender.Atoms.Templates.Query
{
	public static class AtomicQueryTemplateExtensionMethods
	{
		/// <summary>
		/// Query all results into memory by returning them as an IEnumerable
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="parameters">The optional parameters object</param>
		/// <exception cref="ArgumentNullException" />
		/// <exception cref="DbEntityNotFoundException" />
		/// <exception cref="ModelDbEntityMismatchException" />
		/// <exception cref="ModelPropertyTypeMismatchException" />
		/// <exception cref="TaskCanceledException" />
		public static async Task<IEnumerable<T>> QueryAsync<T>(this IAtomicQueryTemplate<T> template, object? parameters = null)
		{
			var results = new List<T>();
			var lazyData = template.QueryLazy(parameters);
			await foreach (var result in lazyData)
				results.Add(result);
			return results;
		}
	}
}
