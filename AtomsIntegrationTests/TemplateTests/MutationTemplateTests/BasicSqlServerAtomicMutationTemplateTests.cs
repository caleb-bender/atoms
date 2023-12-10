using AtomsIntegrationTests.Models;
using CalebBender.Atoms.Repositories;
using CalebBender.Atoms.Repositories.Factories;
using CalebBender.Atoms.Templates.Builders;
using CalebBender.Atoms.Templates.Mutation;
using CalebBender.Atoms.Templates.Query;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AtomsIntegrationTests.DatabaseConfig.SqlServer.SqlServerConnection;

namespace AtomsIntegrationTests.TemplateTests.MutationTemplateTests
{
	[Collection("SqlServerDBTests")]
	public class BasicSqlServerAtomicMutationTemplateTests : BasicAtomicMutationTemplateTests
	{
		protected override void Cleanup()
		{
			using SqlConnection connection = new SqlConnection(GetConnectionString());
			connection.Open();
			using SqlCommand command = new SqlCommand(
				"DELETE FROM TheBlogUsers; DELETE FROM BlogPosts; DELETE FROM CustomerOrders;",
				connection
			);
			command.ExecuteNonQuery();
		}

		protected override async Task<IEnumerable<CustomerOrder>> GetAllCustomerOrders()
		{
			var getAllCustomerOrdersQueryTemplate = new SqlServerRawTemplateBuilder()
				.SetConnectionString(GetConnectionString())
				.SetQueryText("SELECT * FROM CustomerOrders")
				.GetQueryTemplate<CustomerOrder>();
			return await getAllCustomerOrdersQueryTemplate.QueryAsync();
		}

		protected override IAtomicRepository<T> GetAtomicRepository<T>()
		{
			return new SqlServerAtomicRepositoryFactory<T>().CreateRepository(GetConnectionString());
		}

		protected override IAtomicMutationTemplate GetDeleteBlogUserMutationTemplate()
		{
			return new SqlServerRawTemplateBuilder()
				.SetConnectionString(GetConnectionString())
				.SetMutationText("DELETE FROM TheBlogUsers WHERE UserId = @UserId")
				.GetMutationTemplate();
		}

		protected override IAtomicMutationTemplate GetMutationTemplateWithCustomCancellationToken()
		{
			return new SqlServerRawTemplateBuilder()
				.SetConnectionString(GetConnectionString())
				.SetMutationText("UPDATE TheBlogUsers SET UserRole = 'Reader'")
				.SetCancellationToken(customCancellationTokenSource.Token)
				.GetMutationTemplate();
		}

		protected override IAtomicMutationTemplate GetMutationTemplateWithCustomExceptionHandler()
		{
			return new SqlServerRawTemplateBuilder()
				.SetConnectionString(GetConnectionString())
				.SetMutationText($"syntax error")
				.SetExceptionHandler(CustomExceptionHandler)
				.GetMutationTemplate();
		}

		protected override IAtomicMutationTemplate GetUpdateFulfillmentTypeMutationTemplate()
		{
			return new SqlServerRawTemplateBuilder()
				.SetConnectionString(GetConnectionString())
				.SetMutationText("UPDATE CustomerOrders SET OrderType = @FulfillmentType")
				.GetMutationTemplate();
		}

		protected override IAtomicMutationTemplate GetUpdateSingleBlogPostGenreMutationTemplate()
		{
			return new SqlServerRawTemplateBuilder()
				.SetConnectionString(GetConnectionString())
				.SetMutationText("UPDATE BlogPosts SET Genre = @NewGenre WHERE PostId = @PostId AND Genre = @Genre")
				.GetMutationTemplate();
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
