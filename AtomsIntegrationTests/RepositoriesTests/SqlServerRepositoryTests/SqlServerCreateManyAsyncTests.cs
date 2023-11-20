using CalebBender.Atoms.Repositories;
using CalebBender.Atoms.Repositories.Factories;
using CalebBender.Atoms.Utils;
using AtomsIntegrationTests.Models;
using AtomsIntegrationTests.RepositoriesTests.AtomicRepositoryTests;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AtomsIntegrationTests.DatabaseConfig.SqlServer.SqlServerConnection;
using static AtomsIntegrationTests.Models.CustomerOrder;

namespace AtomsIntegrationTests.RepositoriesTests.SqlServerRepositoryTests
{
	[Collection("SqlServerDBTests")]
	public class SqlServerCreateManyAsyncTests : CreateManyAsyncTests
	{
		private readonly IAtomicRepository<CustomerOrder> customerOrderRepo;

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
		{
			customerOrderRepo = new SqlServerAtomicRepositoryFactory<CustomerOrder>()
				.CreateRepository(GetConnectionString());
		}

		[Theory]
		[InlineData("PC", FulfillmentTypes.PickupByCustomer)]
		[InlineData("D", FulfillmentTypes.Delivery)]
		[InlineData("PT", FulfillmentTypes.PickupByThirdParty)]
		[InlineData("NONE", FulfillmentTypes.Unknown)]
		public async Task GivenAModelContainsStringToEnumMappingRulesForAnEnumProperty_WhenWeCreateIt_ThenThoseMappingsAreSavedToTheDatabase(
			string? expectedDatabasePropertyValue,
			FulfillmentTypes enumVariant
		)
		{
			// Arrange
			var orderId = Guid.NewGuid();
			var customerOrder = new CustomerOrder { OrderId = orderId, FulfillmentType = enumVariant };
			// Act
			await customerOrderRepo.CreateOneAsync(customerOrder);
			// Assert
			using SqlConnection connection = new SqlConnection(GetConnectionString());
			string actualDatabasePropertyValue = (await connection.QueryAsync<string>($"SELECT OrderType FROM CustomerOrders WHERE OrderId = '{orderId}'")).First();
			Assert.Equal(expectedDatabasePropertyValue, actualDatabasePropertyValue);
		}

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
