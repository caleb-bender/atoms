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
using static AtomsIntegrationTests.Models.BlogUser;

namespace AtomsIntegrationTests.RepositoriesTests.SqlServerRepositoryTests
{
	[Collection("SqlServerDBTests")]
	public class SqlServerGetOneAsyncTests : GetOneAsyncTests
	{
		public SqlServerGetOneAsyncTests()
		: base(
				new SqlServerAtomicRepositoryFactory<BlogPostAuthor>(),
				new SqlServerAtomicRepositoryFactory<TypeMismatchModel>(),
				new SqlServerAtomicRepositoryFactory<BlogUser>(),
				GetConnectionString()
		) { }

		protected override void Cleanup()
		{
			using SqlConnection connection = new SqlConnection(GetConnectionString());
			connection.Open();
			using SqlCommand deleteCommand = new SqlCommand(
				@"DELETE FROM BlogPostAuthors; DELETE FROM TypeMismatchModels; DELETE FROM TheBlogUsers;",
				connection
			);
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

		protected override async Task CreateOneBlogUserAsync(long userId, string groupName, string? userRole = null)
		{
			if (userRole is null) userRole = BlogUserRole.Reader.ToString();
			using SqlConnection connection = new SqlConnection(GetConnectionString());
			connection.Open();
			using SqlCommand createCommand = new SqlCommand(
				$@"INSERT INTO TheBlogUsers(UserId, GroupId, UserRole)
				VALUES ({userId}, '{groupName}', '{userRole}')", connection
			);
			await createCommand.ExecuteNonQueryAsync();
		}

		protected override async Task CreateOneTypeMismatchModelAsync(Guid id, string status)
		{
			using SqlConnection connection = new SqlConnection(GetConnectionString());
			connection.Open();
			using SqlCommand createCommand = new SqlCommand(
				$@"INSERT INTO TypeMismatchModels(Id, Status)
				VALUES ('{id}', '{status}')", connection
			);
			await createCommand.ExecuteNonQueryAsync();
		}
	}
}
