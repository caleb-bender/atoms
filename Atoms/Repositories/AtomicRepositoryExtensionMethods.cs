using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Repositories
{
	public static class AtomicRepositoryExtensionMethods
	{
		/// <summary>
		/// Extension method for passing in models as params inline
		/// </summary>
		public static async Task<IEnumerable<TModel>> CreateManyAsync<TModel>(this IAtomicRepository<TModel> repo, params TModel[] models)
			where TModel : class, new()
		{
			return await repo.CreateManyAsync(models);
		}

		/// <summary>
		/// Extension method for passing in one model
		/// </summary>
		public static async Task<TModel> CreateOneAsync<TModel>(this IAtomicRepository<TModel> repo, TModel model)
			where TModel : class, new()
		{
			if (model is null) throw new ArgumentNullException("A null model cannot be created with CreateOneAsync.");
			return (await repo.CreateManyAsync(model)).First();
		}

		/// <summary>
		/// Extension method for passing in models as params inline
		/// </summary>
		public static async Task DeleteManyAsync<TModel>(this IAtomicRepository<TModel> repo, params TModel[] models)
			where TModel : class, new()
		{
			await repo.DeleteManyAsync(models);
		}

		/// <summary>
		/// Extension method for passing in one model
		/// </summary>
		public static async Task DeleteOneAsync<TModel>(this IAtomicRepository<TModel> repo, TModel model)
			where TModel : class, new()
		{
			await repo.DeleteManyAsync(model);
		}
	}
}
