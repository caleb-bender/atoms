using Atoms.Repositories;
using Atoms.Templates.Query;
using Atoms.Utils;
using AtomsIntegrationTests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AtomsIntegrationTests.Models.BlogPost;
using static AtomsIntegrationTests.Models.CustomerOrder;

namespace AtomsIntegrationTests.TemplateTests.QueryTemplateTests
{
	public abstract class AtomicQueryTemplateModelTests : IDisposable
	{
		[Fact]
		public async Task GivenThereAreNoBlogPostAuthors_WhenWeLazilyQueryBlogPostAuthors_ThenAnEmptyAsyncEnumerableIsReturned()
		{
			// Arrange
			var authorQueryTemplate = GetAtomicQueryTemplate<BlogPostAuthor>();
			// Act
			var lazyAuthors = authorQueryTemplate.QueryLazy();
			// Assert
			int numberOfResults = 0;
			await foreach (var author in lazyAuthors)
				numberOfResults++;
			Assert.Equal(0, numberOfResults);
		}

		[Fact]
		public async Task GivenSomeBlogPostAuthorsExist_WhenWeLazilyQueryBlogPostAuthors_ThenTheyAreRetrieved()
		{
			// Arrange
			var blogPostAuthors = new[]
			{
				new BlogPostAuthor { AuthorId = 1L, AuthorName = "Bob Doe", AuthorSinceDate = DateTime.Today },
				new BlogPostAuthor { AuthorId = 2L, AuthorName = "Jane Smith", AuthorSinceDate = DateTime.Today }
			};
			var blogPostAuthorRepo = GetAtomicRepository<BlogPostAuthor>();
			await blogPostAuthorRepo.CreateManyAsync(blogPostAuthors);
			var blogPostAuthorTemplate = GetAtomicQueryTemplate<BlogPostAuthor>();
			// Act
			var lazyAuthors = blogPostAuthorTemplate.QueryLazy();
			// Assert
			int i = 0;
			await foreach (var author in lazyAuthors)
			{
				Assert.Equal(blogPostAuthors[i].AuthorName, author.AuthorName);
				i++;
			}
		}

		[Fact]
		public async Task GivenModelWithDifferentNameFromDatabase_WhenWeAttemptToLazilyQueryModels_ThenTheCorrectModelsAreRetrieved()
		{
			// Arrange
			var blogUser1 = new BlogUser { UserId = 1L, GroupName = "Group 1" };
			var blogUser2 = new BlogUser { UserId = 1L, GroupName = "Group 2" };
			var blogUser3 = new BlogUser { UserId = 1L, GroupName = "Group 3" };
			var blogUserRepo = GetAtomicRepository<BlogUser>();
			var blogUserQueryTemplate = GetAtomicQueryTemplate<BlogUser>();
			await blogUserRepo.CreateManyAsync(blogUser1, blogUser2, blogUser3);
			// Act
			var lazyUsers = blogUserQueryTemplate.QueryLazy();
			// Assert
			int i = 1;
			await foreach (var user in lazyUsers)
				Assert.Equal($"Group {i++}", user.GroupName);
		}

		[Fact]
		public async Task GivenACustomerAddressWithSomeNullValues_WhenWeAttemptToLazilyQuery_ThenItsPropertiesAreCorrect()
		{
			// Arrange
			var customerAddress = new CustomerAddress { PhoneNumber = "+11234567890", City = "Los Angeles", Country = "United States" };
			var customerAddressRepo = GetAtomicRepository<CustomerAddress>();
			await customerAddressRepo.CreateOneAsync(customerAddress);
			var customerAddressQueryTemplate = GetAtomicQueryTemplate<CustomerAddress>();
			// Act
			var lazyCustomerAddresses = customerAddressQueryTemplate.QueryLazy();
			// Assert
			await foreach (var address in lazyCustomerAddresses)
			{
				Assert.Equal("+11234567890", address.PhoneNumber);
				Assert.Null(address.UnitNumber);
				Assert.Null(address.StreetNumber);
				Assert.Null(address.Street);
				Assert.Null(address.PostalCode);
				Assert.NotNull(address.City);
				Assert.Null(address.Province);
				Assert.NotNull(address.Country);
				break;
			}
		}

		[Fact]
		public async Task GivenABlogPostWithSerializedBlogComments_WhenWeQueryThem_ThenTheyAreCorrect()
		{
			// Arrange
			var blogComments = new List<BlogComment>
			{
				new BlogComment { Username = "john-doe", Content = "Great short story!" },
				new BlogComment { Username = "janesmith123", Content = "A decent indie tale." }
			};
			var blogPostRepo = GetAtomicRepository<BlogPost>();
			var blogPostQueryTemplate = GetAtomicQueryTemplate<BlogPost>();
			await blogPostRepo.CreateOneAsync(new BlogPost
			{
				PostId = 1L,
				Genre = BlogPostGenre.Fantasy,
				Title = "A Lonely Village",
				Content = "Once upon a time...",
				BlogComments = blogComments
			});
			// Act
			var blogPosts = await blogPostQueryTemplate.QueryAsync();
			// Assert
			var blogPost = blogPosts.First();
			Assert.NotNull(blogPost.BlogComments);
			Assert.Equal(2, blogPost.BlogComments?.Count());
			Assert.Equal("john-doe", blogPost.BlogComments?.First().Username);
		}

		[Theory]
		[InlineData(FulfillmentTypes.PickupByCustomer)]
		[InlineData(FulfillmentTypes.Delivery)]
		[InlineData(FulfillmentTypes.PickupByThirdParty)]
		[InlineData(FulfillmentTypes.Unknown)]
		public async Task GivenACustomerOrderWithVariousDifferentOrderTypeStrings_WhenWeQueryIt_ThenAllEnumVariantsShouldBeMappedCorrectly(
			FulfillmentTypes expectedEnumVariant
		)
		{
			// Arrange
			var orderId = Guid.NewGuid();
			var customerOrderRepo = GetAtomicRepository<CustomerOrder>();
			await customerOrderRepo.CreateOneAsync(new CustomerOrder { OrderId = orderId, FulfillmentType = expectedEnumVariant });
			var customerOrderQueryTemplate = GetAtomicQueryTemplate<CustomerOrder>();
			// Act
			var customerOrders = await customerOrderQueryTemplate.QueryAsync();
			// Assert
			var customerOrder = customerOrders.First();
			Assert.Equal(expectedEnumVariant, customerOrder.FulfillmentType);
		}

		[Fact]
		public async Task GivenModelHasPropertiesWithAtomsIgnoreAttribute_WhenWeQueryIt_ThenIgnoredPropertiesAreSetCorrectly()
		{
			// Arrange
			await CreateOneModelWithIgnoredAsync(1L, "value from database", "value from database");
			var modelWithIgnoredQueryTemplate = GetAtomicQueryTemplate<ModelWithIgnored>();
			// Act
			var modelsWithIgnored = await modelWithIgnoredQueryTemplate.QueryAsync();
			// Assert
			var modelWithIgnoredFirst = modelsWithIgnored.First();
			Assert.Equal("value from database", modelWithIgnoredFirst.PropertyReadFromButNotWrittenTo);
			Assert.Equal("default", modelWithIgnoredFirst.PropertyNeitherReadFromNorWrittenTo);
		}

		public void Dispose()
		{
			Cleanup();
		}
		protected abstract void Cleanup();
		protected abstract IAtomicQueryTemplate<T> GetAtomicQueryTemplate<T>() where T : class, new();
		protected abstract IAtomicRepository<T> GetAtomicRepository<T>() where T: class, new();
		protected abstract Task CreateOneModelWithIgnoredAsync(long id, string propertyReadFromButNotWrittenTo, string propertyNeitherReadFromNorWrittenTo);
	}
}
