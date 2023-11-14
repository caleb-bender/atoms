using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Repositories
{
	public static class AtomicRepositoryExtensionMethods
	{
		public static async Task<IEnumerable<TModel>> CreateManyAsync<TModel>(this IAtomicRepository<TModel> repo, params TModel[] models)
			where TModel : class, new()
		{
			return await repo.CreateManyAsync(models);
		}
	}
}
