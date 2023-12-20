using CalebBender.Atoms.Repositories.Factories;
using CalebBender.Atoms.Repositories;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AtomsIntegrationTests.DatabaseConfig.SqlServer.SqlServerConnection;
using AtomsIntegrationTests.Models;
using CalebBender.Atoms.Templates.Builders;
using CalebBender.Atoms.Templates.Query;
using static AtomsIntegrationTests.Models.BlogPost;

namespace AtomsIntegrationTests.TemplateTests.QueryTemplateTests
{
	[Collection("SqlServerDBTests")]
	public class IEnumerableParametersSqlServerAtomicQueryTemplateTests : IEnumerableParametersTemplateTests
	{
		[Fact]
		public async Task GivenSomeBlogPosts_WhenWeQueryThemUsingTemplate_ThenCorrectBlogPostsAreReturned()
		{
			// Arrange
			var genres = new[]
			{
				BlogPostGenre.Scifi,
				BlogPostGenre.Horror,
				BlogPostGenre.Thriller
			};
			await blogPostRepo.CreateManyAsync(blogPosts);
			var blogPostQueryTemplate = GetBlogPostQueryTemplate();
			// Act
			var lazyBlogPosts = blogPostQueryTemplate.QueryLazy(new { Genres = genres });
			// Assert
			await foreach (var blogPost in lazyBlogPosts)
				Assert.NotEqual(BlogPostGenre.Fantasy, blogPost.Genre);
		}

		[Fact]
		public async Task GivenSomeCustomerOrders_WhenWeQueryThemUsingTemplate_ThenCorrectCustomerOrdersAreReturned()
		{
			// Arrange
			var FulfillmentTypes = new List<CustomerOrder.FulfillmentTypes>
			{
				CustomerOrder.FulfillmentTypes.Unknown,
				CustomerOrder.FulfillmentTypes.PickupByThirdParty,
				CustomerOrder.FulfillmentTypes.PickupByCustomer
			};
			await customerOrderRepo.CreateManyAsync(customerOrders);
			var customerOrderQueryTemplate = GetCustomerOrderQueryTemplate();
			// Act
			var queriedCustomerOrders = await customerOrderQueryTemplate.QueryAsync(new { FulfillmentTypes });
			// Assert
			Assert.Single(queriedCustomerOrders);
			Assert.Equal(CustomerOrder.FulfillmentTypes.Delivery, queriedCustomerOrders.First().FulfillmentType);
		}


		[Fact]
		public async Task GivenMultipleInAndNotInClausesWithParameters_WhenWeQueryUsingTemplate_ThenCorrectResultsAreReturned()
		{
			// Arrange
			await blogPostRepo.CreateManyAsync(blogPosts);
			var blogPostQueryTemplateWithMultipleClauses = GetBlogPostQueryTemplateWithMultipleClauses();
			var Genres = new[] { BlogPostGenre.Horror, BlogPostGenre.Scifi };
			var Titles = new[] { "2" };
			// Act
			var queriedBlogPosts =
				await blogPostQueryTemplateWithMultipleClauses.QueryAsync(new { Genres, Titles });
			// Assert
			Assert.Single(queriedBlogPosts);
			var queriedBlogPost = queriedBlogPosts.First();
			Assert.Equal(BlogPostGenre.Scifi, queriedBlogPost.Genre);
			Assert.Equal("4", queriedBlogPost.Title);
		}

		[Fact]
		public async Task GivenExtraWhitespaceWithInClause_WhenWeQueryUsingTemplate_ThenCorrectResultsAreReturned()
		{
			// Arrange
			await blogPostRepo.CreateManyAsync(blogPosts);
			var blogPostQueryTemplateWithExtraWhitespace = GetBlogPostQueryTemplateWithExtraWhitespace();
			var Genres = new[] { BlogPostGenre.Horror, BlogPostGenre.Thriller };
			// Act
			var queriedBlogPosts = await blogPostQueryTemplateWithExtraWhitespace.QueryAsync(new { Genres });
			// Assert
			Assert.Equal(2, queriedBlogPosts.Count());
			Assert.Equal(2L, queriedBlogPosts.ElementAt(0).PostId);
			Assert.Equal(3L, queriedBlogPosts.ElementAt(1).PostId);
		}

		private static IAtomicQueryTemplate<CustomerOrder> GetCustomerOrderQueryTemplate()
		{
			return new SqlServerRawTemplateBuilder()
				.SetConnectionString(GetConnectionString())
				.SetQueryText("SELECT * FROM CustomerOrders WHERE OrderType NOT IN @FulfillmentTypes")
				.GetQueryTemplate<CustomerOrder>();
		}

		private static IAtomicQueryTemplate<BlogPost> GetBlogPostQueryTemplate()
		{
			return new SqlServerRawTemplateBuilder()
				.SetConnectionString(GetConnectionString())
				.SetQueryText("SELECT * FROM BlogPosts WHERE Genre IN @Genres")
				.GetQueryTemplate<BlogPost>();
		}

		private static IAtomicQueryTemplate<BlogPost> GetBlogPostQueryTemplateWithMultipleClauses()
		{
			return new SqlServerRawTemplateBuilder()
				.SetConnectionString(GetConnectionString())
				.SetQueryText("SELECT * FROM BlogPosts WHERE Genre IN @Genres AND Title not in @Titles")
				.GetQueryTemplate<BlogPost>();
		}

		private static IAtomicQueryTemplate<BlogPost> GetBlogPostQueryTemplateWithExtraWhitespace()
		{
			return new SqlServerRawTemplateBuilder()
				.SetConnectionString(GetConnectionString())
				.SetQueryText("SELECT * FROM BlogPosts WHERE Genre IN   @Genres")
				.GetQueryTemplate<BlogPost>();
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
