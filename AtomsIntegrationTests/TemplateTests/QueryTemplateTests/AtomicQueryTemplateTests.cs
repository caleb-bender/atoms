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
	public abstract class AtomicQueryTemplateTests : IDisposable
	{
		private readonly IAtomicRepository<BlogPost> blogPostRepo;
		private readonly IAtomicQueryTemplate<long> blogPostIdQueryTemplate;
		private readonly IAtomicQueryTemplate<(long, BlogPost.BlogPostGenre, string)> blogIdGenreTitleQueryTemplate;
		private readonly IAtomicRepository<CustomerOrder> customerOrderRepo;
		private readonly IAtomicQueryTemplate<(Guid, CustomerOrder.FulfillmentTypes)> customerOrderIdAndTypeQueryTemplate;
		private readonly IAtomicQueryTemplate<string?> customerCityQueryTemplate;
		IAtomicRepository<CustomerAddress> customerAddressRepo;
		private BlogPost blogPost1 = new BlogPost { PostId = 1L, Genre = BlogPost.BlogPostGenre.Horror, Title = "The Cursed Baby" };
		private BlogPost blogPost2 = new BlogPost { PostId = 2L, Genre = BlogPost.BlogPostGenre.Scifi, Title = "Cosmic Quest" };
		private BlogPost blogPost3 = new BlogPost { PostId = 3L, Genre = BlogPost.BlogPostGenre.Fantasy, Title = "Kingdoms of the Hallowed" };

		protected AtomicQueryTemplateTests(
			IAtomicQueryTemplate<long> blogPostIdQueryTemplate,
			IAtomicQueryTemplate<(long, BlogPost.BlogPostGenre, string)> blogIdGenreTitleQueryTemplate,
			IAtomicQueryTemplate<(Guid, CustomerOrder.FulfillmentTypes)> customerOrderIdAndTypeQueryTemplate,
			IAtomicQueryTemplate<string?> customerCityQueryTemplate
			)
		{
			blogPostRepo = GetAtomicRepository<BlogPost>();
			this.blogPostIdQueryTemplate = blogPostIdQueryTemplate;
			this.blogIdGenreTitleQueryTemplate = blogIdGenreTitleQueryTemplate;
			customerOrderRepo = GetAtomicRepository<CustomerOrder>();
			this.customerOrderIdAndTypeQueryTemplate = customerOrderIdAndTypeQueryTemplate;
			this.customerCityQueryTemplate = customerCityQueryTemplate;
			customerAddressRepo = GetAtomicRepository<CustomerAddress>();
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


		public void Dispose()
		{
			Cleanup();
		}

		protected abstract void Cleanup();

		protected abstract IAtomicRepository<T> GetAtomicRepository<T>() where T : class, new();
	}
}
