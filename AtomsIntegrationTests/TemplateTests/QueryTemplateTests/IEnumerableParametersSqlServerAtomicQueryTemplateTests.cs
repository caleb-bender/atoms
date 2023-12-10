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
	public class IEnumerableParametersSqlServerAtomicQueryTemplateTests : IDisposable
	{
		private readonly IAtomicRepository<BlogPost> blogPostRepo;
		private readonly BlogPost[] blogPosts = new[]
		{
			new BlogPost { PostId = 1L, Genre = BlogPostGenre.Fantasy, Title = "1" },
			new BlogPost { PostId = 2L, Genre = BlogPostGenre.Horror, Title = "2" },
			new BlogPost { PostId = 3L, Genre = BlogPostGenre.Thriller, Title = "3" },
			new BlogPost { PostId = 4L, Genre = BlogPostGenre.Scifi, Title = "4" }
		};

		private readonly IAtomicRepository<CustomerOrder> customerOrderRepo;
		private readonly List<CustomerOrder> customerOrders = new List<CustomerOrder>
		{
			new CustomerOrder { OrderId = Guid.NewGuid(), FulfillmentType = CustomerOrder.FulfillmentTypes.Unknown },
			new CustomerOrder { OrderId = Guid.NewGuid(), FulfillmentType = CustomerOrder.FulfillmentTypes.PickupByThirdParty },
			new CustomerOrder { OrderId = Guid.NewGuid(), FulfillmentType = CustomerOrder.FulfillmentTypes.PickupByCustomer },
			new CustomerOrder { OrderId = Guid.NewGuid(), FulfillmentType = CustomerOrder.FulfillmentTypes.Delivery }
		};

		public IEnumerableParametersSqlServerAtomicQueryTemplateTests()
		{
			blogPostRepo = GetAtomicRepository<BlogPost>();
			customerOrderRepo = GetAtomicRepository<CustomerOrder>();
		}


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
			var Titles = new[] { "1", "2" };
			// Act
			var queriedBlogPosts =
				await blogPostQueryTemplateWithMultipleClauses.QueryAsync(new { Genres, Titles });
			// Assert
			Assert.Single(queriedBlogPosts);
			var queriedBlogPost = queriedBlogPosts.First();
			Assert.Equal(BlogPostGenre.Scifi, queriedBlogPost.Genre);
			Assert.Equal("4", queriedBlogPost.Title);
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

		private IAtomicRepository<T> GetAtomicRepository<T>() where T : class, new()
		{
			return new SqlServerAtomicRepositoryFactory<T>().CreateRepository(GetConnectionString());
		}

		private void Cleanup()
		{
			using SqlConnection connection = new SqlConnection(GetConnectionString());
			connection.Open();
			using SqlCommand command = new SqlCommand(
				"DELETE FROM BlogPosts; DELETE FROM CustomerOrders;",
				connection
			);
			command.ExecuteNonQuery();
		}

		public void Dispose()
		{
			Cleanup();
		}
	}
}
