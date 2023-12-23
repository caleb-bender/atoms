using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalebBender.Atoms.Templates.Mutation
{
	/// <summary>
	/// A mutation template with immutable state used for reusing a constant
	/// write operation or operations with variable parameters.
	/// </summary>
	public interface IAtomicMutationTemplate
	{
		/// <summary>
		/// Executes the write operation or operations defined on the instance's
		/// creation with the specified parameters
		/// </summary>
		/// <param name="parameters">
		/// An object with public properties matching the names of the parameters
		/// referenced by the template
		/// </param>
		/// <returns>An integer representing the number of entries that were modified</returns>
		/// <exception cref="ArgumentNullException" />
		/// <exception cref="TaskCanceledException" />
		Task<int> MutateAsync(object? parameters = null);
	}
}
