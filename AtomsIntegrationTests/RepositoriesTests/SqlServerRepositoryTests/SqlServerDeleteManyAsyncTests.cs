using CalebBender.Atoms.Repositories.Factories;
using AtomsIntegrationTests.Models;
using AtomsIntegrationTests.RepositoriesTests.AtomicRepositoryTests;
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
	public class SqlServerDeleteManyAsyncTests : DeleteManyAsyncTests
	{

		public SqlServerDeleteManyAsyncTests()
		: base (
			new SqlServerAtomicRepositoryFactory<CustomerAddress>(),
			new SqlServerAtomicRepositoryFactory<BlogUser>(),
			new SqlServerAtomicRepositoryFactory<BlogPost>(),
			new SqlServerAtomicRepositoryFactory<NonexistentModel>(),
			new SqlServerAtomicRepositoryFactory<JobPostingModelEntityMismatch>(),
			new SqlServerAtomicRepositoryFactory<TypeMismatchModel3>(),
			GetConnectionString()
		)
		{

		}

		protected override void Cleanup()
		{
			using SqlConnection connection = new SqlConnection(GetConnectionString());
			connection.Open();
			using SqlCommand deleteCommand = new SqlCommand(
				@"DELETE FROM CustomerAddresses; DELETE FROM TheBlogUsers;
				DELETE FROM BlogPosts; DELETE FROM JobPostings;", connection
			);
			deleteCommand.ExecuteNonQuery();
		}
	}
}
