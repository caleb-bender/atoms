using AtomsIntegrationTests.Models;
using CalebBender.Atoms.Repositories;
using CalebBender.Atoms.Repositories.Factories;
using CalebBender.Atoms.Templates.Builders;
using CalebBender.Atoms.Utils;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AtomsIntegrationTests.DatabaseConfig.SqlServer.SqlServerConnection;
using static AtomsIntegrationTests.Models.BlogPost;

namespace AtomsIntegrationTests.TemplateTests.MutationTemplateTests
{
	[Collection("SqlServerDBTests")]
	public class IEnumerableParametersSqlServerAtomicMutationTemplateTests : IEnumerableParametersTemplateTests
	{

		[Fact]
		public async Task GivenSomeBlogPosts_WhenWeDeleteSomeUsingTemplate_ThenCorrectBlogPostsAreDeleted()
		{
			// Arrange
			await blogPostRepo.CreateManyAsync(blogPosts);
			var blogPostDeletionTemplate = new SqlServerRawTemplateBuilder()
				.SetConnectionString(GetConnectionString())
				.SetMutationText("DELETE FROM BlogPosts WHERE Genre IN @Genres")
				.GetMutationTemplate();
			var Genres = new[] { BlogPostGenre.Horror, BlogPostGenre.Thriller, BlogPostGenre.Scifi };
			// Act
			var numberDeleted = await blogPostDeletionTemplate.MutateAsync(new { Genres });
			Assert.Equal(3, numberDeleted);
			var queriedBlogPostOption = await blogPostRepo.GetOneAsync(blogPosts[0]);
			Assert.IsType<AtomicOption<BlogPost>.Exists>(queriedBlogPostOption);
		}

		[Fact]
		public async Task GivenSomeCustomerOrders_WhenWeUpdateSomeUsingTemplate_ThenCorrectCustomerOrdersAreUpdated()
		{
			// Arrange
			await customerOrderRepo.CreateManyAsync(customerOrders);
			var customerOrderUpdateTemplate = new SqlServerRawTemplateBuilder()
				.SetConnectionString(GetConnectionString())
				.SetMutationText("UPDATE CustomerOrders SET OrderType = @NewFulfillmentType WHERE OrderType IN @FulfillmentTypes")
				.GetMutationTemplate();
			var FulfillmentTypes = new[] {
				CustomerOrder.FulfillmentTypes.Unknown,
				CustomerOrder.FulfillmentTypes.PickupByCustomer,
				CustomerOrder.FulfillmentTypes.PickupByThirdParty
			};
			// Act
			var numberUpdated =
				await customerOrderUpdateTemplate.MutateAsync(
					new { NewFulfillmentType = CustomerOrder.FulfillmentTypes.Delivery, FulfillmentTypes }	
				);
			// Assert
			Assert.Equal(3, numberUpdated);
		}

		[Fact]
		public async Task GivenSomeBlogPosts_WhenWeDeleteSomeUsingMultipleInAndNotInClauses_ThenTheCorrectOnesAreDeleted()
		{
			// Arrange
			await blogPostRepo.CreateManyAsync(blogPosts);
			var blogPostDeletionTemplate = new SqlServerRawTemplateBuilder()
				.SetConnectionString(GetConnectionString())
				.SetMutationText("DELETE FROM BlogPosts WHERE Genre IN @Genres AND Title not in @Titles")
				.GetMutationTemplate();
			var Genres = new[] { BlogPostGenre.Horror, BlogPostGenre.Thriller, BlogPostGenre.Scifi };
			var Titles = new[] { "4" };
			// Act
			var numberDeleted = await blogPostDeletionTemplate.MutateAsync(new { Genres, Titles });
			Assert.Equal(2, numberDeleted);
		}


		protected override IAtomicRepository<T> GetAtomicRepository<T>()
		{
			return new SqlServerAtomicRepositoryFactory<T>().CreateRepository(GetConnectionString());
		}

		protected override void Cleanup()
		{
			using SqlConnection connection = new SqlConnection(GetConnectionString());
			connection.Open();
			using SqlCommand command = new SqlCommand(
				"DELETE FROM BlogPosts; DELETE FROM CustomerOrders;",
				connection
			);
			command.ExecuteNonQuery();
		}
	}
}
