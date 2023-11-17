using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Templates.Mutation
{
	public interface IAtomicMutationTemplate
	{
		Task MutateAsync(object? parameters = null);
	}
}
