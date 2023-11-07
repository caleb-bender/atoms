using Atoms.Exceptions;
using Atoms.Repositories;
using Atoms.Repositories.Factories;
using Atoms.Utils;
using AtomsIntegrationTests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtomsIntegrationTests.RepositoriesTests.FactoriesTests
{
    public abstract class AtomicRepositoryFactoryTests
    {
        private readonly IAtomicRepositoryFactory<ModelWithUniqueIdAttribute> atomicRepositoryFactory;
		private readonly IAtomicRepositoryFactory<ModelWithoutUniqueIdAttribute> atomicRepositoryFactoryWithoutModelUniqueId;
		private readonly string validConnectionString;

		public AtomicRepositoryFactoryTests(
            IAtomicRepositoryFactory<ModelWithUniqueIdAttribute> atomicRepositoryFactory,
			IAtomicRepositoryFactory<ModelWithoutUniqueIdAttribute> atomicRepositoryFactoryWithoutModelUniqueId,
            string validConnectionString
        )
        {
            this.atomicRepositoryFactory = atomicRepositoryFactory;
			this.atomicRepositoryFactoryWithoutModelUniqueId = atomicRepositoryFactoryWithoutModelUniqueId;
			this.validConnectionString = validConnectionString;
		}

		[Fact]
		public void WhenWePassValidConnectionStringToAtomicRepositoryFactory_ThenAnAtomicRepositoryIsReturned()
		{
			// Act
			IAtomicRepository<ModelWithUniqueIdAttribute> repository = atomicRepositoryFactory.CreateRepository(validConnectionString);
			// Assert
			Assert.NotNull(repository);
		}

		[Fact]
		public void WhenWePassInvalidConnectionStringToAtomicRepositoryFactory_ThenAnAtomsConnectionExceptionIsThrown()
		{
			// Assert
			Assert.Throws<AtomsConnectionException>(() =>
			{
				// Act
				var repositoryResult = atomicRepositoryFactory.CreateRepository("");
			});
		}

		[Fact]
		public void WhenWeUseModelWithoutUniqueIdAttributeForGenericType_ThenAMissingUniqueIdAttributeExceptionIsThrown()
		{
			// Assert
			Assert.Throws<MissingUniqueIdAttributeException>(() =>
			{
				// Act
				var repository = 
					atomicRepositoryFactoryWithoutModelUniqueId.CreateRepository(validConnectionString);
			});
		}

	}
}
