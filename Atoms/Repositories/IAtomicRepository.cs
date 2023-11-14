using Atoms.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Repositories
{
	public interface IAtomicRepository<TModel> where TModel : class, new()
	{
		Task<IEnumerable<TModel>> CreateManyAsync(IEnumerable<TModel> models);
		Task<AtomicOption<TModel>> GetOneAsync(TModel model);
	}
}
