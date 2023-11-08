using Atoms.DataAttributes;
using Atoms.Exceptions;
using Atoms.Utils;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Atoms.Utils.Reflection.AttributeCheckerHelpers;
using static Atoms.Utils.Reflection.TypeCheckingHelpers;

namespace Atoms.Repositories.Factories
{
	public abstract class AtomicRepositoryFactory<TModel> : IAtomicRepositoryFactory<TModel>
		where TModel : class, new()
	{
		private static readonly bool modelTypeDoesNotContainUniqueIdAttribute =
			!HasAttributeOnAtLeastOneProperty<UniqueIdAttribute>(typeof(TModel));

		private static readonly PropertyInfo? firstPropertyWithUniqueIdThatIsIncompatible = GetFirstPropertyWithUniqueIdThatIsIncompatibleOrNull();
		public IAtomicRepository<TModel> CreateRepository(string dbConnectionString)
		{
			if (modelTypeDoesNotContainUniqueIdAttribute)
				ThrowMissingUniqueIdAttributeException();
			else if (firstPropertyWithUniqueIdThatIsIncompatible is not null)
				ThrowPropertyTypeIsIncompatibleWithUniqueIdAttributeException();
			AttemptToConnectAndOpen(dbConnectionString);
			return NewAtomicRepositoryResult(dbConnectionString);
		}

		private static void ThrowPropertyTypeIsIncompatibleWithUniqueIdAttributeException()
		{
			throw new PropertyTypeIsIncompatibleWithUniqueIdAttributeException($"The UniqueIdAttribute cannot be used for the property \"{typeof(TModel).Name}.{firstPropertyWithUniqueIdThatIsIncompatible.Name}\" because its type is not compatible.");
		}

		private static void ThrowMissingUniqueIdAttributeException()
		{
			throw new MissingUniqueIdAttributeException(
				$@"The model type ""{nameof(TModel)}"" must contain the Atoms.DataAttributes.UniqueIdAttribute on at least one of its public properties."
			);
		}

		private static PropertyInfo? GetFirstPropertyWithUniqueIdThatIsIncompatibleOrNull()
		{
			return GetPublicPropertiesThatContainAttribute<UniqueIdAttribute>(typeof(TModel))
				.Where(property => !IsCompatibleUniqueIdType(property.PropertyType))
				.FirstOrDefault();
		}

		protected abstract IAtomicRepository<TModel> NewAtomicRepositoryResult(string connectionString);
		protected abstract void AttemptToConnectAndOpen(string connectionString);
	}
}
