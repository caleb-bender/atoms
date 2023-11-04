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
		public void WhenWePassValidConnectionStringToAtomicRepositoryFactory_ThenAnOkAtomicResultIsReturned()
		{
			// Act
			var repositoryResult = atomicRepositoryFactory.CreateRepository(validConnectionString);
			// Assert
			var okResult = Assert.IsType<AtomicResult<IAtomicRepository<ModelWithUniqueIdAttribute>, AtomsException>.Ok>(repositoryResult);
			Assert.NotNull(okResult.Value);
		}

		[Fact]
		public void WhenWePassInvalidConnectionStringToAtomicRepositoryFactory_ThenAnErrorAtomicResultIsReturned()
		{
			// Act
			var repositoryResult = atomicRepositoryFactory.CreateRepository("");
			// Assert
			var errorResult = Assert.IsType<AtomicResult<IAtomicRepository<ModelWithUniqueIdAttribute>, AtomsException>.Error>(repositoryResult);
			Assert.True(errorResult.Except is AtomsConnectionException);
		}

		[Fact]
		public void WhenWeUseModelWithoutUniqueIdAttributeForGenericType_ThenAnErrorAtomicResultIsReturned()
		{
			// Act
			var repositoryResult = 
				atomicRepositoryFactoryWithoutModelUniqueId.CreateRepository(validConnectionString);
			// Assert
			var errorResult = Assert.IsType<AtomicResult<IAtomicRepository<ModelWithoutUniqueIdAttribute>, AtomsException>.Error>(repositoryResult);
			Assert.True(errorResult.Except is MissingUniqueIdAttributeException);
		}

	}
}
