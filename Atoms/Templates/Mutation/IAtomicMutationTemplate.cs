using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalebBender.Atoms.Templates.Mutation
{
	public interface IAtomicMutationTemplate
	{
		Task<int> MutateAsync(object? parameters = null);
	}
}
