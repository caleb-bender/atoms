using CalebBender.Atoms.Repositories.Factories;
using CalebBender.Atoms.Repositories;
using CalebBender.Atoms.Templates.Query;
using AtomsIntegrationTests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using static AtomsIntegrationTests.DatabaseConfig.SqlServer.SqlServerConnection;
using CalebBender.Atoms.Templates.Builders;

namespace AtomsIntegrationTests.TemplateTests.QueryTemplateTests
{
	[Collection("SqlServerDBTests")]
	public class SqlServerAtomicQueryTemplateMappingErrorsTests : AtomicQueryTemplateMappingErrorsTests
	{
		private static readonly string connectionString = GetConnectionString();
		private static readonly IAtomicQueryTemplate<NonexistentModel> nonexistentModelTemplate =
			GetNonexistentModelTemplate();
		private static readonly IAtomicQueryTemplate<JobPostingModelEntityMismatch>
			jobPostingModelEntityMismatchTemplate = GetJobPostingMismatchTemplate();
		private static readonly IAtomicQueryTemplate<TypeMismatchModel3> typeMismatchModelTemplate =
			GetTypeMismatchTemplate();
		public SqlServerAtomicQueryTemplateMappingErrorsTests()
		: base(nonexistentModelTemplate, jobPostingModelEntityMismatchTemplate, typeMismatchModelTemplate)
		{
		}

		private static IAtomicQueryTemplate<NonexistentModel> GetNonexistentModelTemplate()
		{
			return new SqlServerRawTemplateBuilder()
				.SetConnectionString(connectionString)
				.SetQueryText("SELECT * FROM NonexistentModels")
				.GetQueryTemplate<NonexistentModel>();
		}

		private static IAtomicQueryTemplate<JobPostingModelEntityMismatch> GetJobPostingMismatchTemplate()
		{
			return new SqlServerRawTemplateBuilder()
				.SetConnectionString(connectionString)
				.SetQueryText("SELECT * FROM JobPostings")
				.GetQueryTemplate<JobPostingModelEntityMismatch>();
		}

		private static IAtomicQueryTemplate<TypeMismatchModel3> GetTypeMismatchTemplate()
		{
			return new SqlServerRawTemplateBuilder()
				.SetConnectionString(connectionString)
				.SetQueryText("SELECT * FROM TypeMismatchModels")
				.GetQueryTemplate<TypeMismatchModel3>();
		}

		protected override void Cleanup()
		{
			using SqlConnection connection = new SqlConnection(connectionString);
			connection.Open();
			using SqlCommand command = new SqlCommand(
				@"DELETE FROM JobPostings; DELETE FROM TypeMismatchModels;",
				connection
			);
			command.ExecuteNonQuery();
		}

		protected override IAtomicRepository<T> GetAtomicRepository<T>()
		{
			return new SqlServerAtomicRepositoryFactory<T>().CreateRepository(connectionString);
		}
	}
}
