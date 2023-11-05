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
	public class SqlServerGetOneAsyncTests : GetOneAsyncTests
	{
		public SqlServerGetOneAsyncTests()
		: base(
				new SqlServerAtomicRepositoryFactory<BlogPostAuthor>(),
				GetConnectionString()
		) { }

		protected override void Cleanup()
		{
			using SqlConnection connection = new SqlConnection(GetConnectionString());
			connection.Open();
			using SqlCommand deleteCommand = new SqlCommand("DELETE FROM BlogPostAuthors", connection);
			deleteCommand.ExecuteNonQuery();
		}

		protected override async Task CreateOneBlogPostAuthorAsync(long authorId, string authorName, DateTime authorSinceDate)
		{
			using SqlConnection connection = new SqlConnection(GetConnectionString());
			connection.Open();
			using SqlCommand createCommand = new SqlCommand(
				$@"INSERT INTO BlogPostAuthors(AuthorId, AuthorName, UserRegisteredOn)
				VALUES ({authorId}, '{authorName}', '{authorSinceDate}')", connection
			);
			await createCommand.ExecuteNonQueryAsync();
		}
	}
}
