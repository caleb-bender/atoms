﻿using CalebBender.Atoms.Exceptions;
using CalebBender.Atoms.Repositories;
using CalebBender.Atoms.Repositories.Factories;
using CalebBender.Atoms.Utils;
using AtomsIntegrationTests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AtomsIntegrationTests.DatabaseConfig.SqlServer.SqlServerConnection;

namespace AtomsIntegrationTests.RepositoriesTests.FactoriesTests
{
    public class SqlServerAtomicRepositoryFactoryTests : AtomicRepositoryFactoryTests
    {
        SqlServerAtomicRepositoryFactory<ModelWithMultipleAutoGeneratedUniqueIds> repositoryWithMultipleAutoGeneratedFactory;
        SqlServerAtomicRepositoryFactory<ModelWithGuidAutoGeneratedId> repositoryWithGuidAutoGeneratedFactory;
        static string connectionString = GetConnectionString();

        public SqlServerAtomicRepositoryFactoryTests() :
        base(
            new SqlServerAtomicRepositoryFactory<ModelWithUniqueIdAttribute>(),
            new SqlServerAtomicRepositoryFactory<ModelWithoutUniqueIdAttribute>(),
            new SqlServerAtomicRepositoryFactory<ModelWithPropertyNotCompatibleWithUniqueId>(),
            new SqlServerAtomicRepositoryFactory<ModelWithNullableUniqueId>(),
            connectionString
        )
        {
            repositoryWithMultipleAutoGeneratedFactory = new SqlServerAtomicRepositoryFactory<ModelWithMultipleAutoGeneratedUniqueIds>();
            repositoryWithGuidAutoGeneratedFactory = new SqlServerAtomicRepositoryFactory<ModelWithGuidAutoGeneratedId>();
		}

        [Fact]
        public void WhenWeAttemptToCreateAFactoryForAModelWithMoreThanOneAutoGeneratedUniqueId_ThenATooManyAutoGeneratedUniqueIdsExceptionIsThrown()
        {
            // Assert
            Assert.Throws<TooManyAutoGeneratedUniqueIdsException>(() =>
            {
                repositoryWithMultipleAutoGeneratedFactory.CreateRepository(connectionString);
            });
        }

        [Fact]
        public void WhenWeAttemptToCreateAFactoryForAModelThatHasAnAutoGeneratedIdThatIsANonInteger_ThenAnAutoGeneratedUniqueIdMustBeAnIntegerTypeExceptionIsThrown()
        {
            // Assert
            Assert.Throws<AutoGeneratedUniqueIdMustBeAnIntegerTypeException>(() =>
            {
                repositoryWithGuidAutoGeneratedFactory.CreateRepository(connectionString);
            });
        }
    }
}
