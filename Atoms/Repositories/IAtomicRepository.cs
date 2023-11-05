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
		Task<AtomicOption<TModel>> GetOneAsync(TModel modelWithUniqueIdSet);
	}
}
