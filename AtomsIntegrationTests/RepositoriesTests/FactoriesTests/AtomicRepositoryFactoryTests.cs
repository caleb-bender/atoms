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
		private readonly IAtomicRepositoryFactory<ModelWithoutUniqueIdAttribute> atomicRepositoryWithoutModelUniqueIdFactory;
		private readonly IAtomicRepositoryFactory<ModelWithPropertyNotCompatibleWithUniqueId> atomicRepositoryWithPropertyNotCompatibleWithUniqueIdFactory;
		private readonly IAtomicRepositoryFactory<ModelWithNullableUniqueId> atomicRepositoryWithNullableIdFactory;
		private readonly string validConnectionString;

		public AtomicRepositoryFactoryTests(
            IAtomicRepositoryFactory<ModelWithUniqueIdAttribute> atomicRepositoryFactory,
			IAtomicRepositoryFactory<ModelWithoutUniqueIdAttribute> atomicRepositoryWithoutModelUniqueIdFactory,
			IAtomicRepositoryFactory<ModelWithPropertyNotCompatibleWithUniqueId> atomicRepositoryWithPropertyNotCompatibleWithUniqueIdFactory,
			IAtomicRepositoryFactory<ModelWithNullableUniqueId> atomicRepositoryWithNullableIdFactory,
            string validConnectionString
        )
        {
            this.atomicRepositoryFactory = atomicRepositoryFactory;
			this.atomicRepositoryWithoutModelUniqueIdFactory = atomicRepositoryWithoutModelUniqueIdFactory;
			this.atomicRepositoryWithPropertyNotCompatibleWithUniqueIdFactory = atomicRepositoryWithPropertyNotCompatibleWithUniqueIdFactory;
			this.atomicRepositoryWithNullableIdFactory = atomicRepositoryWithNullableIdFactory;
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
					atomicRepositoryWithoutModelUniqueIdFactory.CreateRepository(validConnectionString);
			});
		}

		[Fact]
		public void WhenWeUseModelWithPropertyNotCompatibleWithUniqueId_ThenAPropertyTypeIsIncompatibleWithUniqueIdAttributeExceptionIsThrown()
		{
			// Assert
			Assert.Throws<PropertyTypeIsIncompatibleWithUniqueIdAttributeException>(() =>
			{
				// Act
				atomicRepositoryWithPropertyNotCompatibleWithUniqueIdFactory.CreateRepository(validConnectionString);
			});
		}

		[Fact]
		public void WhenWeUseModelWithNullablleUniqueId_ThenAPropertyTypeIsIncompatibleWithUniqueIdAttributeExceptionIsThrown()
		{
			// Assert
			Assert.Throws<PropertyTypeIsIncompatibleWithUniqueIdAttributeException>(() =>
			{
				// Act
				atomicRepositoryWithNullableIdFactory.CreateRepository(validConnectionString);
			});
		}

	}
}
