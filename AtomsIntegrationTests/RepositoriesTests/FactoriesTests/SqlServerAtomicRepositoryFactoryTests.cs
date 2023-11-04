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
using static AtomsIntegrationTests.DatabaseConfig.SqlServer.SqlServerConnection;

namespace AtomsIntegrationTests.RepositoriesTests.FactoriesTests
{
    public class SqlServerAtomicRepositoryFactoryTests : AtomicRepositoryFactoryTests
    {
        IAtomicRepositoryFactory<ModelWithoutUniqueIdAttribute> sqlServerRepoFactory;

        public SqlServerAtomicRepositoryFactoryTests() :
        base(
            new SqlServerAtomicRepositoryFactory<ModelWithUniqueIdAttribute>(),
            new SqlServerAtomicRepositoryFactory<ModelWithoutUniqueIdAttribute>(),
            GetConnectionString()
        )
        { }
    }
}
