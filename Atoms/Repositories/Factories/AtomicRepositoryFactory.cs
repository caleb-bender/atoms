using Atoms.DataAttributes;
using Atoms.Exceptions;
using Atoms.Utils;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Atoms.Utils.Reflection.AttributeCheckerHelpers;

namespace Atoms.Repositories.Factories
{
	public abstract class AtomicRepositoryFactory<TModel> : IAtomicRepositoryFactory<TModel>
		where TModel : class, new()
	{
		private static readonly bool modelTypeDoesNotContainUniqueIdAttribute =
			!HasAttributeOnAtLeastOneProperty<UniqueIdAttribute>(typeof(TModel));
		public IAtomicRepository<TModel> CreateRepository(string dbConnectionString)
		{
			if (modelTypeDoesNotContainUniqueIdAttribute)
				ThrowNewMissingUniqueIdAttributeException();
			AttemptToConnectAndOpen(dbConnectionString);
			return NewAtomicRepositoryResult(dbConnectionString);
		}

		private static void ThrowNewMissingUniqueIdAttributeException()
		{
			throw new MissingUniqueIdAttributeException(
				$@"The model type ""{nameof(TModel)}"" must contain the Atoms.DataAttributes.UniqueIdAttribute on at least one of its public properties."
			);
		}

		protected abstract IAtomicRepository<TModel> NewAtomicRepositoryResult(string connectionString);
		protected abstract void AttemptToConnectAndOpen(string connectionString);
	}
}
