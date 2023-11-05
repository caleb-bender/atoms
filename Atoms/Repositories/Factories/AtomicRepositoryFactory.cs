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
		public AtomicResult<IAtomicRepository<TModel>, AtomsException> CreateRepository(string dbConnectionString)
		{
			try
			{
				if (modelTypeDoesNotContainUniqueIdAttribute)
					return MissingUniqueIdAttributeExceptionResult();
				AttemptToConnectAndOpen(dbConnectionString);
				return NewAtomicRepositoryResult(dbConnectionString);
			}
			catch (Exception err)
			{
				return AtomsConnectionExceptionResult(err);
			}
		}

		protected abstract AtomicResult<IAtomicRepository<TModel>, AtomsException> NewAtomicRepositoryResult(string connectionString);
		protected abstract void AttemptToConnectAndOpen(string connectionString);

		protected virtual AtomicResult<IAtomicRepository<TModel>, AtomsException> AtomsConnectionExceptionResult(Exception err)
		{
			var connectionException = new AtomsConnectionException(err.Message, err);
			return new AtomicResult<IAtomicRepository<TModel>, AtomsException>
				.Error(connectionException);
		}

		private static AtomicResult<IAtomicRepository<TModel>, AtomsException>
		MissingUniqueIdAttributeExceptionResult()
		{
			return new AtomicResult<IAtomicRepository<TModel>, AtomsException>.Error(
				new MissingUniqueIdAttributeException($"The model type \"{nameof(TModel)}\" must contain the Atoms.DataAttributes.UniqueIdAttribute on at least one of its public properties.")
			);
		}
	}
}
