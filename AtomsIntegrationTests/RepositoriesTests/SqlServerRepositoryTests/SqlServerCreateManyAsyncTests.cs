using Atoms.Repositories.Factories;
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
	public class SqlServerCreateManyAsyncTests : CreateManyAsyncTests
	{
		public SqlServerCreateManyAsyncTests()
			: base(
				  new SqlServerAtomicRepositoryFactory<BlogPostAuthor>(),
				  new SqlServerAtomicRepositoryFactory<CustomerAddress>(),
				  new SqlServerAtomicRepositoryFactory<JobPosting>(),
				  new SqlServerAtomicRepositoryFactory<Employee>(),
				  new SqlServerAtomicRepositoryFactory<BlogUser>(),
				  new SqlServerAtomicRepositoryFactory<ModelWithIgnored>(),
				  new SqlServerAtomicRepositoryFactory<JobPostingModelEntityMismatch>(),
				  new SqlServerAtomicRepositoryFactory<NonexistentModel>(),
				  new SqlServerAtomicRepositoryFactory<TypeMismatchModel>(),
				  new SqlServerAtomicRepositoryFactory<BlogPost>(),
				  GetConnectionString()
			)
		{}

		protected override void Cleanup()
		{
			using SqlConnection connection = new SqlConnection(GetConnectionString());
			connection.Open();
			using SqlCommand deleteCommand = new SqlCommand(
				@"DELETE FROM BlogPostAuthors; DELETE FROM CustomerAddresses;
				DELETE FROM JobPostings; DELETE FROM Employees; DELETE FROM TheBlogUsers;
				DELETE FROM ModelsWithIgnored; DELETE FROM TypeMismatchModels;
				DELETE FROM BlogPosts;",
				connection
			);
			deleteCommand.ExecuteNonQuery();
		}
	}
}
