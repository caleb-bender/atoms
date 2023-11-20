using Atoms.Repositories;
using Atoms.Repositories.Factories;
using Atoms.Repositories.SqlServer;
using Atoms.Repositories.SqlServer.SqlGeneration;
using Atoms.Templates.Builders;
using Atoms.Templates.Query;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AtomsIntegrationTests.DatabaseConfig.SqlServer.SqlServerConnection;

namespace AtomsIntegrationTests.TemplateTests.QueryTemplateTests
{
	[Collection("SqlServerDBTests")]
	public class SqlServerAtomicQueryTemplateModelTests : AtomicQueryTemplateModelTests
	{
		private static readonly string connectionString = GetConnectionString();

		protected override void Cleanup()
		{
			using SqlConnection connection = new SqlConnection(connectionString);
			connection.Open();
			using SqlCommand command = new SqlCommand(
				@"DELETE FROM BlogPostAuthors; DELETE FROM TypeMismatchModels;
				DELETE FROM TheBlogUsers; DELETE FROM BlogPosts;
				DELETE FROM CustomerAddresses; DELETE FROM CustomerOrders;
				DELETE FROM ModelsWithIgnored; DELETE FROM JobPostings;",
				connection
			);
			command.ExecuteNonQuery();
		}

		protected override IAtomicQueryTemplate<T> GetAtomicQueryTemplate<T>()
		{
			return new SqlServerRawTemplateBuilder()
				.SetConnectionString(connectionString)
				.SetQueryText($"SELECT * FROM [{ModelMetadata<T>.TableName}]")
				.GetQueryTemplate<T>();
		}

		protected override IAtomicRepository<T> GetAtomicRepository<T>()
		{
			return new SqlServerAtomicRepositoryFactory<T>().CreateRepository(connectionString);
		}

		protected override async Task CreateOneModelWithIgnoredAsync(long id, string propertyReadFromButNotWrittenTo, string propertyNeitherReadFromNorWrittenTo)
		{
			using SqlConnection connection = new SqlConnection(connectionString);
			connection.Open();
			using SqlCommand createCommand = new SqlCommand(
				$@"INSERT INTO ModelsWithIgnored(Id, PropertyReadFromButNotWrittenTo, PropertyNeitherReadFromNorWrittenTo)
				VALUES ('{id}', '{propertyReadFromButNotWrittenTo}', '{propertyNeitherReadFromNorWrittenTo}')", connection
			);
			await createCommand.ExecuteNonQueryAsync();
		}
	}
}
