using Atoms.Repositories;
using Atoms.Repositories.Factories;
using Atoms.Templates.Query;
using AtomsIntegrationTests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtomsIntegrationTests.TemplateTests.QueryTemplateTests
{
	public abstract class BasicAtomicQueryTemplateTests : IDisposable
	{
		protected CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		private static bool exceptionHandlerWasCalled = false;
		private readonly IAtomicRepository<BlogPost> blogPostRepo;
		private readonly IAtomicQueryTemplate<long> blogPostIdQueryTemplate;
		private readonly IAtomicQueryTemplate<(long, BlogPost.BlogPostGenre, string)> blogIdGenreTitleQueryTemplate;
		private readonly IAtomicRepository<CustomerOrder> customerOrderRepo;
		private readonly IAtomicQueryTemplate<(Guid, CustomerOrder.FulfillmentTypes)> customerOrderIdAndTypeQueryTemplate;
		private readonly IAtomicQueryTemplate<string?> customerCityQueryTemplate;
		private readonly IAtomicQueryTemplate<BlogPost.BlogPostGenre> blogPostGenresTemplate;
		private readonly IAtomicQueryTemplate<CustomerOrder.FulfillmentTypes> customerOrderTypesTemplate;
		private readonly IAtomicQueryTemplate<string> blogPostTitleWithSpecificGenreAndTitleStartsWithLetterTemplate;
		private readonly IAtomicQueryTemplate<(Guid, CustomerOrder.FulfillmentTypes)> customerOrderIdAndTypeWithSpecificFulfillmentTypeTemplate;
		IAtomicRepository<CustomerAddress> customerAddressRepo;
		private readonly IAtomicQueryTemplate<DateTime> queryTemplateThatResultsInExceptionBeingThrown;
		private readonly IAtomicQueryTemplate<BlogPost.BlogPostGenre> blogPostGenreTemplateWithCancelToken;
		private BlogPost blogPost1 = new BlogPost { PostId = 1L, Genre = BlogPost.BlogPostGenre.Horror, Title = "The Cursed Baby" };
		private BlogPost blogPost2 = new BlogPost { PostId = 2L, Genre = BlogPost.BlogPostGenre.Scifi, Title = "Cosmic Quest" };
		private BlogPost blogPost3 = new BlogPost { PostId = 3L, Genre = BlogPost.BlogPostGenre.Fantasy, Title = "Kingdoms of the Hallowed" };
		private BlogPost blogPost4 = new BlogPost { PostId = 4L, Genre = BlogPost.BlogPostGenre.Fantasy, Title = "Castle of the Sea" };

		protected BasicAtomicQueryTemplateTests(
			IAtomicQueryTemplate<long> blogPostIdQueryTemplate,
			IAtomicQueryTemplate<(long, BlogPost.BlogPostGenre, string)> blogIdGenreTitleQueryTemplate,
			IAtomicQueryTemplate<(Guid, CustomerOrder.FulfillmentTypes)> customerOrderIdAndTypeQueryTemplate,
			IAtomicQueryTemplate<string?> customerCityQueryTemplate,
			IAtomicQueryTemplate<BlogPost.BlogPostGenre> blogPostGenresTemplate,
			IAtomicQueryTemplate<CustomerOrder.FulfillmentTypes> customerOrderTypesTemplate,
			IAtomicQueryTemplate<string> blogPostTitleWithSpecificGenreAndTitleStartsWithLetterTemplate,
			IAtomicQueryTemplate<(Guid, CustomerOrder.FulfillmentTypes)> customerOrderIdAndTypeWithSpecificFulfillmentTypeTemplate,
			IAtomicQueryTemplate<DateTime> queryTemplateThatResultsInExceptionBeingThrown
		)
		{
			blogPostRepo = GetAtomicRepository<BlogPost>();
			this.blogPostIdQueryTemplate = blogPostIdQueryTemplate;
			this.blogIdGenreTitleQueryTemplate = blogIdGenreTitleQueryTemplate;
			customerOrderRepo = GetAtomicRepository<CustomerOrder>();
			this.customerOrderIdAndTypeQueryTemplate = customerOrderIdAndTypeQueryTemplate;
			this.customerCityQueryTemplate = customerCityQueryTemplate;
			this.blogPostGenresTemplate = blogPostGenresTemplate;
			this.customerOrderTypesTemplate = customerOrderTypesTemplate;
			this.blogPostTitleWithSpecificGenreAndTitleStartsWithLetterTemplate = blogPostTitleWithSpecificGenreAndTitleStartsWithLetterTemplate;
			this.customerOrderIdAndTypeWithSpecificFulfillmentTypeTemplate = customerOrderIdAndTypeWithSpecificFulfillmentTypeTemplate;
			customerAddressRepo = GetAtomicRepository<CustomerAddress>();
			this.queryTemplateThatResultsInExceptionBeingThrown = queryTemplateThatResultsInExceptionBeingThrown;
			blogPostGenreTemplateWithCancelToken = GetBlogGenreQueryTemplateWithCancelToken();
		}

		[Fact]
		public async Task WhenWeLazilyQueryACollectionOfPrimitives_ThenWeGetBackTheCorrectValues()
		{
			// Arrange
			await blogPostRepo.CreateManyAsync(blogPost1, blogPost2, blogPost3);
			// Act
			var lazyBlogPostIds = blogPostIdQueryTemplate.QueryLazy();
			// Assert
			long expectedPostId = 1L;
			await foreach (var blogPostId in lazyBlogPostIds)
			{
				Assert.Equal(expectedPostId++, blogPostId);
			}
		}

		[Fact]
		public async Task WhenWeLazilyQueryAValueTupleWithEnum_ThenWeGetBackTheCorrectValues()
		{
			// Arrange
			var blogPosts = new[] { blogPost1, blogPost2, blogPost3 };
			await blogPostRepo.CreateManyAsync(blogPosts);
			// Act
			var lazyIdsGenresTitles = blogIdGenreTitleQueryTemplate.QueryLazy();
			// Assert
			int i = 0;
			await foreach (var (id, genre, title) in lazyIdsGenresTitles)
			{
				Assert.Equal(blogPosts[i].PostId, id);
				Assert.Equal(blogPosts[i].Genre, genre);
				Assert.Equal(blogPosts[i].Title, title);
				i++;
			}
		}

		[Fact]
		public async Task WhenWeLazilyQueryAValueTupleWithEnumContainingMappingRules_ThenWeGetBackTheCorrectValues()
		{
			// Arrange
			var customerOrder1 = new CustomerOrder { OrderId = Guid.NewGuid(), FulfillmentType = CustomerOrder.FulfillmentTypes.PickupByThirdParty };
			var customerOrder2 = new CustomerOrder { OrderId = Guid.NewGuid(), FulfillmentType = CustomerOrder.FulfillmentTypes.Unknown };
			var customerOrder3 = new CustomerOrder { OrderId = Guid.NewGuid(), FulfillmentType = CustomerOrder.FulfillmentTypes.PickupByCustomer };
			var customerOrders = new[] { customerOrder1, customerOrder2, customerOrder3 };
			await customerOrderRepo.CreateManyAsync(customerOrders);
			// Act
			var lazyOrderIdsAndTypes = customerOrderIdAndTypeQueryTemplate.QueryLazy();
			// Assert
			await foreach (var (orderId, fulfillmentType) in lazyOrderIdsAndTypes)
			{
				Assert.Single(customerOrders.Where(x => x.OrderId == orderId));
				Assert.Single(customerOrders.Where(x => x.FulfillmentType == fulfillmentType));
			}
		}

		[Fact]
		public async Task WhenWeLazilyQueryAValueThatIsSometimesNull_ThenWeGetBackTheCorrectValues()
		{
			// Arrange
			var customerAddress1 = new CustomerAddress { PhoneNumber = "1234567890", City = "Sacramento" };
			var customerAddress2 = new CustomerAddress { PhoneNumber = "1234567891" };
			var customerAddresses = new[] { customerAddress1, customerAddress2 };
			await customerAddressRepo.CreateManyAsync(customerAddresses);
			// Act
			var lazyCustomerCities = customerCityQueryTemplate.QueryLazy();
			// Assert
			int i = 0;
			await foreach (var cityName in lazyCustomerCities)
			{
				if (i == 0) Assert.Equal("Sacramento", cityName);
				else Assert.Null(cityName);
				i++;
			}
		}

		[Fact]
		public async Task WhenWeLazilyQueryEnumScalarThatDoesNotContainMappingRules_ThenWeGetBackCorrectValues()
		{
			// Arrange
			await blogPostRepo.CreateOneAsync(blogPost2);
			// Act
			var lazyBlogPostGenres = blogPostGenresTemplate.QueryLazy();
			await foreach (var genre in lazyBlogPostGenres)
				Assert.Equal(blogPost2.Genre, genre);
		}

		[Fact]
		public async Task WhenWeLazilyQueryEnumScalarThatContainsMappingRules_ThenWeGetBackCorrectValues()
		{
			// Arrange
			var customerOrder1 = new CustomerOrder { OrderId = Guid.NewGuid(), FulfillmentType = CustomerOrder.FulfillmentTypes.PickupByThirdParty };
			var customerOrder3 = new CustomerOrder { OrderId = Guid.NewGuid(), FulfillmentType = CustomerOrder.FulfillmentTypes.PickupByCustomer };
			var customerOrders = new[] { customerOrder1, customerOrder3 };
			await customerOrderRepo.CreateManyAsync(customerOrders);
			// Act
			var lazyFulfillmentTypes = customerOrderTypesTemplate.QueryLazy();
			// Assert
			await foreach (var fulfillmentType in lazyFulfillmentTypes)
				Assert.Single(customerOrders.Where(x => x.FulfillmentType == fulfillmentType));
		}

		[Fact]
		public async Task WhenWeLazilyQueryDataUsingParameters_AndOneOfTheParametersIsEnumWithoutMappingRules_ThenWeGetBackCorrectData()
		{
			// Arrange
			await blogPostRepo.CreateManyAsync(blogPost1, blogPost2, blogPost3, blogPost4);
			// Act
			var lazyTitles =
				blogPostTitleWithSpecificGenreAndTitleStartsWithLetterTemplate
				.QueryLazy(new { Genre = BlogPost.BlogPostGenre.Fantasy, Title = "K%" });
			// Assert
			await foreach (var title in lazyTitles)
				Assert.Equal(blogPost3.Title, title);
		}

		[Fact]
		public async Task WhenWeLazilyQueryDataUsingParameters_AndOneOfTheParametersIsEnumWithMappingRules_ThenWeGetBackCorrectData()
		{
			// Arrange
			var customerOrder1 = new CustomerOrder { OrderId = Guid.NewGuid(), FulfillmentType = CustomerOrder.FulfillmentTypes.PickupByThirdParty };
			var customerOrder3 = new CustomerOrder { OrderId = Guid.NewGuid(), FulfillmentType = CustomerOrder.FulfillmentTypes.PickupByCustomer };
			await customerOrderRepo.CreateManyAsync(customerOrder1, customerOrder3);
			// Act
			var lazyOrderIdsAndTypes =
				customerOrderIdAndTypeWithSpecificFulfillmentTypeTemplate
				.QueryLazy(new { OrderType = CustomerOrder.FulfillmentTypes.PickupByCustomer });
			// Assert
			int numberOfResults = 0;
			await foreach (var (id, fulfillmentType) in lazyOrderIdsAndTypes)
			{
				numberOfResults++;
				Assert.Equal(customerOrder3.OrderId, id);
				Assert.Equal(customerOrder3.FulfillmentType, fulfillmentType);
			}
			Assert.Equal(1, numberOfResults);
		}

		[Fact]
		public async Task GivenAnExceptionHandlerIsDefinedOnTheTemplate_WhenWeLazilyQueryThatResultsInAnExceptionBeingThrown_ThenTheHandlerIsCalled()
		{
			// Arrange
			var customerAddress1 = new CustomerAddress { PhoneNumber = "1234567890", UnitNumber = 1 };
			await customerAddressRepo.CreateOneAsync(customerAddress1);
			var lazyData = queryTemplateThatResultsInExceptionBeingThrown.QueryLazy();
			// Act
			await foreach (var data in lazyData) { }
			// Assert
			Assert.True(exceptionHandlerWasCalled);
		}

		[Fact]
		public async Task GivenADefinedCancellationTokenOnTheTemplate_WhenWeCancelBeforeLazilyQueryingData_ThenTaskCanceledExceptionIsThrown()
		{
			// Arrange
			await blogPostRepo.CreateManyAsync(blogPost1, blogPost2);
			// Act
			var lazyGenres = blogPostGenreTemplateWithCancelToken.QueryLazy();
			cancellationTokenSource.Cancel();
			// Assert
			await Assert.ThrowsAsync<TaskCanceledException>(async () =>
			{
				await foreach (var genre in lazyGenres) { }
			});
		}

		[Fact]
		public async Task WhenWeQueryAllBlogPostTitlesUpFront_ThenTheResultsAreCorrect()
		{
			// Arrange
			await blogPostRepo.CreateManyAsync(blogPost1, blogPost2, blogPost3, blogPost4);
			// Act
			var titles =
				await blogPostTitleWithSpecificGenreAndTitleStartsWithLetterTemplate
				.QueryAsync(new { Genre = BlogPost.BlogPostGenre.Fantasy, Title = "K%" });
			// Assert
			Assert.Equal(1, titles.Count());
			foreach (var title in titles)
				Assert.Equal(blogPost3.Title, title);
		}

		protected static Task ExceptionHandler(Exception err)
		{
			exceptionHandlerWasCalled = true;
			return Task.CompletedTask;
		}


		public void Dispose()
		{
			exceptionHandlerWasCalled = false;
			Cleanup();
		}

		protected abstract void Cleanup();

		protected abstract IAtomicRepository<T> GetAtomicRepository<T>() where T : class, new();
		protected abstract IAtomicQueryTemplate<BlogPost.BlogPostGenre> GetBlogGenreQueryTemplateWithCancelToken();
	}
}
