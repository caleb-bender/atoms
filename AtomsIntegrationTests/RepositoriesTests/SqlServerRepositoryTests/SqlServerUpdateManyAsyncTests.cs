using AtomsIntegrationTests.RepositoriesTests.AtomicRepositoryTests;
using CalebBender.Atoms.Repositories;
using CalebBender.Atoms.Repositories.Factories;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AtomsIntegrationTests.DatabaseConfig.SqlServer.SqlServerConnection;

namespace AtomsIntegrationTests.RepositoriesTests.SqlServerRepositoryTests
{
	[Collection("SqlServerDBTests")]
	public class SqlServerUpdateManyAsyncTests : UpdateManyAsyncTests
	{
		protected override void Cleanup()
		{
			using SqlConnection connection = new SqlConnection(GetConnectionString());
			connection.Open();
			using SqlCommand deleteCommand = new SqlCommand(
				@"DELETE FROM Employees; DELETE FROM CustomerAddresses; DELETE FROM BlogPosts;",
				connection);
			deleteCommand.ExecuteNonQuery();
		}

		protected override IAtomicRepository<T> CreateRepository<T>()
		{
			return new SqlServerAtomicRepositoryFactory<T>().CreateRepository(GetConnectionString());
		}
	}
}
