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
	public class SqlServerAtomicQueryTemplateModelTests : AtomicQueryTemplateModelTests
	{
		private static readonly string connectionString = GetConnectionString();

		protected override void Cleanup()
		{
			using SqlConnection connection = new SqlConnection(connectionString);
			connection.Open();
			using SqlCommand command = new SqlCommand(
				@"DELETE FROM BlogPostAuthors;"
			);
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
	}
}
