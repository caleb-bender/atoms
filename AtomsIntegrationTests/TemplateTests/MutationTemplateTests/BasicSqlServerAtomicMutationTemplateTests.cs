using CalebBender.Atoms.Repositories;
using CalebBender.Atoms.Repositories.Factories;
using CalebBender.Atoms.Templates.Builders;
using CalebBender.Atoms.Templates.Mutation;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AtomsIntegrationTests.DatabaseConfig.SqlServer.SqlServerConnection;

namespace AtomsIntegrationTests.TemplateTests.MutationTemplateTests
{
	public class BasicSqlServerAtomicMutationTemplateTests : BasicAtomicMutationTemplateTests
	{
		protected override void Cleanup()
		{
			using SqlConnection connection = new SqlConnection(GetConnectionString());
			connection.Open();
			using SqlCommand command = new SqlCommand("DELETE FROM TheBlogUsers", connection);
			command.ExecuteNonQuery();
		}

		protected override IAtomicRepository<T> GetAtomicRepository<T>()
		{
			return new SqlServerAtomicRepositoryFactory<T>().CreateRepository(GetConnectionString());
		}

		protected override IAtomicMutationTemplate GetUpdateSingleBlogUserMutationTemplate(long userId, string groupName)
		{
			return new SqlServerRawTemplateBuilder()
				.SetConnectionString(GetConnectionString())
				.SetMutationText($"UPDATE TheBlogUsers SET UserRole = 'Moderator' WHERE UserId = {userId} AND GroupId = '{groupName}';")
				.GetMutationTemplate();
		}
	}
}
