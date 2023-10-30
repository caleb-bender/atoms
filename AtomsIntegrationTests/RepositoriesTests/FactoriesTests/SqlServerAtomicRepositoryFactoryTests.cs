using Atoms.Exceptions;
using Atoms.Repositories;
using Atoms.Repositories.Factories;
using Atoms.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AtomsIntegrationTests.DatabaseConfig.SqlServer.SqlServerConnection;

namespace AtomsIntegrationTests.RepositoriesTests.FactoriesTests
{
    public class SqlServerAtomicRepositoryFactoryTests
    {
        IAtomicRepositoryFactory sqlServerRepoFactory;

        public SqlServerAtomicRepositoryFactoryTests()
        {
            sqlServerRepoFactory = new SqlServerAtomicRepositoryFactory();
        }

        [Fact]
        public void WhenWePassValidConnectionStringToSqlServerAtomicRepositoryFactory_ThenAnOkAtomicResultIsReturned()
        {
            // Act
            var repositoryResult = sqlServerRepoFactory.CreateRepository(GetConnectionString());
            // Assert
            var okResult = Assert.IsType<AtomicResult<IAtomicRepository, AtomsConnectionException>.Ok>(repositoryResult);
            Assert.NotNull(okResult.Value);            
        }

        [Fact]
        public void WhenWePassInvalidConnectionStringToSqlServerAtomicRepositoryFactory_ThenAnErrorAtomicResultIsReturned()
        {
            // Act
            var repositoryResult = sqlServerRepoFactory.CreateRepository("");
            // Assert
            Assert.IsType<AtomicResult<IAtomicRepository, AtomsConnectionException>.Error>(repositoryResult);
        }
    }
}
