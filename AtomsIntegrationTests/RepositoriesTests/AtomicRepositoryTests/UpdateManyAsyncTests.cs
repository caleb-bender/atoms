using AtomsIntegrationTests.Models;
using CalebBender.Atoms.Exceptions;
using CalebBender.Atoms.Repositories;
using CalebBender.Atoms.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtomsIntegrationTests.RepositoriesTests.AtomicRepositoryTests
{
	public abstract class UpdateManyAsyncTests : IDisposable
	{
		[Fact]
		public async Task GivenSomeCreatedEmployees_WhenWeUpdateMany_ThenCorrectOnesAreUpdated()
		{
			// Arrange
			var employeeRepo = CreateRepository<Employee>();
			var createdEmployees = await employeeRepo.CreateManyAsync(
				new Employee { EmployeeId = Guid.NewGuid(), Salary = 90_000M },
				new Employee { EmployeeId = Guid.NewGuid(), Salary = 65_000M },
				new Employee { EmployeeId = Guid.NewGuid(), Salary = 80_000M }
			);
			// Act
			foreach (var employee in createdEmployees)
				employee.Salary = (employee.Salary < 85_000M) ? 70_000M : employee.Salary;
			var numberUpdated = await employeeRepo.UpdateManyAsync(createdEmployees.Skip(1));
			// Assert
			Assert.Equal(2, numberUpdated);
			Assert.Equal(90_000M, (await GetUpdatedModel(createdEmployees.ElementAt(0), employeeRepo)).Salary);
			Assert.Equal(70_000M, (await GetUpdatedModel(createdEmployees.ElementAt(1), employeeRepo)).Salary);
			Assert.Equal(70_000M, (await GetUpdatedModel(createdEmployees.ElementAt(2), employeeRepo)).Salary);
		}

		[Fact]
		public async Task GivenCreatedModelsWithDifferentEntityAndPropertyNames_WhenWeUpdateMany_ThenTheyAreUpdated()
		{
			// Arrange
			var customerAddressRepo = CreateRepository<CustomerAddress>();
			var createdCustomerAddresses = await customerAddressRepo.CreateManyAsync(
				new CustomerAddress { PhoneNumber = "11234567890" },
				new CustomerAddress { PhoneNumber = "10987654321" }
			);
			// Act
			foreach (var customerAddress in createdCustomerAddresses)
				customerAddress.Street = "Oak Street";
			var numberUpdated = await customerAddressRepo.UpdateManyAsync(createdCustomerAddresses);
			// Assert
			Assert.Equal(2, numberUpdated);
			Assert.Equal("Oak Street", (await GetUpdatedModel(createdCustomerAddresses.ElementAt(0), customerAddressRepo)).Street);
			Assert.Equal("Oak Street", (await GetUpdatedModel(createdCustomerAddresses.ElementAt(1), customerAddressRepo)).Street);
		}

		[Fact]
		public async Task GivenSomeCreatedBlogPosts_WhenWeUpdateMany_ThenTheyAreUpdated()
		{
			// Arrange
			var blogPostRepo = CreateRepository<BlogPost>();
			var blogPost1 = new BlogPost { PostId = 1L, Genre = BlogPost.BlogPostGenre.Horror, Title = "An Eery Feeling" };
			var blogPost2 = new BlogPost { PostId = 2L, Genre = BlogPost.BlogPostGenre.Scifi, Title = "Among the Stars" };
			await blogPostRepo.CreateManyAsync(blogPost1, blogPost2);
			// Act
			blogPost2.Title = "Among the Planets";
			var numberUpdated = await blogPostRepo.UpdateManyAsync(blogPost2);
			// Assert
			Assert.Equal(1, numberUpdated);
			Assert.Equal("Among the Planets", (await GetUpdatedModel(blogPost2, blogPostRepo)).Title);
		}

		[Fact]
		public async Task GivenSomeCreatedBlogPosts_WhenWeAttemptToUpdateTheirPostId_NothingIsUpdated()
		{
			// Arrange
			var blogPostRepo = CreateRepository<BlogPost>();
			var blogPost1 = new BlogPost { PostId = 1L, Genre = BlogPost.BlogPostGenre.Horror, Title = "An Eery Feeling" };
			var blogPost2 = new BlogPost { PostId = 2L, Genre = BlogPost.BlogPostGenre.Scifi, Title = "Among the Stars" };
			await blogPostRepo.CreateManyAsync(blogPost1, blogPost2);
			// Act
			blogPost2.PostId = 3L;
			var numberUpdated = await blogPostRepo.UpdateOneAsync(blogPost2);
			// Assert
			Assert.Equal(0, numberUpdated);
		}

		[Fact]
		public async Task GivenANullOrEmptyIEnumerable_WhenWeUpdateMany_ThenNothingIsUpdated()
		{
			// Arrange
			var blogPostRepo = CreateRepository<BlogPost>();

			// Act
			var numberUpdated = await blogPostRepo.UpdateManyAsync(null);

			// Assert
			Assert.Equal(0, numberUpdated);

			// Act
			numberUpdated = await blogPostRepo.UpdateManyAsync(new List<BlogPost>());

			// Assert
			Assert.Equal(0, numberUpdated);
		}

		[Fact]
		public async Task GivenACreatedBlogPost_WhenWeUpdateBlogComments_ThenTheyAreSuccessfullyUpdated()
		{
			// Arrange
			var blogPostRepo = CreateRepository<BlogPost>();
			var blogPost = new BlogPost
			{
				PostId = 1L,
				Genre = BlogPost.BlogPostGenre.Thriller,
				Title = "Lost Identity",
				Content = "In a dreary town called Willow Grove, there was ...",
				BlogComments = new List<BlogComment>
				{
					new BlogComment
					{
						Username = "john-doe",
						Content = "Very capitivating!"
					}
				}
			};
			await blogPostRepo.CreateOneAsync(blogPost);
			// Act
			blogPost.BlogComments.Add(new BlogComment { Username = "jane-smith", Content = "On the edge of my seat reading this!" });
			var numberUpdated = await blogPostRepo.UpdateOneAsync(blogPost);
			// Assert
			Assert.Equal(1, numberUpdated);
			Assert.Equal("jane-smith", (await GetUpdatedModel(blogPost, blogPostRepo)).BlogComments?.Last().Username);
		}

		[Fact]
		public async Task GivenSomeCreatedCustomerOrders_WhenWeUpdateManyFulfillmentTypes_ThenTheyAreUpdatedCorrectly()
		{
			// Arrange
			var customerOrderRepo = CreateRepository<CustomerOrder>();
			var createdCustomerOrders = await customerOrderRepo.CreateManyAsync(
				new CustomerOrder { OrderId = Guid.NewGuid(), FulfillmentType = CustomerOrder.FulfillmentTypes.PickupByThirdParty },
				new CustomerOrder { OrderId = Guid.NewGuid(), FulfillmentType = CustomerOrder.FulfillmentTypes.PickupByCustomer }
			);
			// Act
			var numberUpdated = await customerOrderRepo.UpdateManyAsync(
				createdCustomerOrders.Select(order => { order.FulfillmentType = CustomerOrder.FulfillmentTypes.Delivery; return order; })
			);
			// Assert
			Assert.Equal(2, numberUpdated);
			int i = 0;
			foreach (var customerOrder in createdCustomerOrders)
			{
				var retrievedCustomerOrder = await GetUpdatedModel(createdCustomerOrders.ElementAt(i++), customerOrderRepo);
				Assert.Equal(CustomerOrder.FulfillmentTypes.Delivery, retrievedCustomerOrder.FulfillmentType);
			}
		}

		[Fact]
		public async Task GivenSomeCreatedModelsWithAtomsIgnoreProperty_WhenWeUpdateMany_ThenNoneAreUpdated()
		{
			// Arrange
			var modelWithIgnoredRepo = CreateRepository<ModelWithIgnored>();
			var model = await modelWithIgnoredRepo.CreateOneAsync(
				new ModelWithIgnored { Id = 1L }
			);
			// Act
			await modelWithIgnoredRepo.UpdateOneAsync(
				new ModelWithIgnored {
					Id = 1L,
					PropertyNeitherReadFromNorWrittenTo = "CHANGED",
					PropertyReadFromButNotWrittenTo = "CHANGED",
					PropertyWrittenAtCreationAndReadOnlyThereafter = "CHANGED"
				}
			);
			// Assert
			var retrievedModel = await GetUpdatedModel(model, modelWithIgnoredRepo);
			Assert.True(retrievedModel.PropertyNeitherReadFromNorWrittenTo == "default");
			Assert.True(retrievedModel.PropertyReadFromButNotWrittenTo is null);
			Assert.True(retrievedModel.PropertyWrittenAtCreationAndReadOnlyThereafter == "default");
		}

		[Fact]
		public async Task GivenThereAreNoPropertiesToWriteTo_WhenWeUpdateOne_ThenANoWritableModelPropertiesExistExceptionIsThrown()
		{
			// Arrange
			var oneIdentityPropertyRepo = CreateRepository<OneIdentityPropertyModel>();
			// Assert
			await Assert.ThrowsAsync<NoWritableModelPropertiesExistException>(async () =>
			{
				await oneIdentityPropertyRepo.UpdateOneAsync(new OneIdentityPropertyModel { });
			});
		}

		private async Task<T> GetUpdatedModel<T>(T model, IAtomicRepository<T> repo) where T : class, new()
		{
			var atomicOption = await repo.GetOneAsync(model);
			if (atomicOption is AtomicOption<T>.Exists modelExists)
				return modelExists.Value;
			else
				throw new ArgumentNullException("The model doesn't exist");
		}

		protected abstract IAtomicRepository<T> CreateRepository<T>() where T: class, new();
		protected abstract void Cleanup();

		public void Dispose()
		{
			Cleanup();
		}
	}
}
