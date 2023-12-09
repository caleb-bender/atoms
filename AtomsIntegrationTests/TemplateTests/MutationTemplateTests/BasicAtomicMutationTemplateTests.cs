using AtomsIntegrationTests.Models;
using CalebBender.Atoms.Repositories;
using CalebBender.Atoms.Templates.Mutation;
using CalebBender.Atoms.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtomsIntegrationTests.TemplateTests.MutationTemplateTests
{
	[Collection("SqlServerDBTests")]
	public abstract class BasicAtomicMutationTemplateTests : IDisposable
	{
		private readonly IAtomicRepository<BlogUser> blogUserRepo;
		private readonly IAtomicRepository<BlogPost> blogPostRepo;
		private readonly IAtomicRepository<CustomerOrder> customerOrderRepo;
		private static bool customExceptionHandlerWasCalled = false;
		protected static CancellationTokenSource customCancellationTokenSource = new CancellationTokenSource();

		BlogUser blogUser = new BlogUser { UserId = 1L, GroupName = "Group 1" };
		BlogUser blogUser2 = new BlogUser { UserId = 1L, GroupName = "Group 2" };
		BlogUser blogUser3 = new BlogUser { UserId = 2L, GroupName = "Group 2" };

		BlogPost blogPost = new BlogPost { PostId = 1L, Genre = BlogPost.BlogPostGenre.Horror };
		BlogPost blogPost2 = new BlogPost { PostId = 2L, Genre = BlogPost.BlogPostGenre.Scifi };

		CustomerOrder customerOrder = new CustomerOrder { OrderId = Guid.NewGuid(), FulfillmentType = CustomerOrder.FulfillmentTypes.Unknown };
		CustomerOrder customerOrder2 = new CustomerOrder { OrderId = Guid.NewGuid(), FulfillmentType = CustomerOrder.FulfillmentTypes.Unknown };
		CustomerOrder customerOrder3 = new CustomerOrder { OrderId = Guid.NewGuid(), FulfillmentType = CustomerOrder.FulfillmentTypes.Unknown };

		public BasicAtomicMutationTemplateTests()
		{
			blogUserRepo = GetAtomicRepository<BlogUser>();
			blogPostRepo = GetAtomicRepository<BlogPost>();
			customerOrderRepo = GetAtomicRepository<CustomerOrder>();
		}

		[Fact]
		public async Task GivenAFewBlogUsers_WhenWeExecuteAnUpdateSingleMutationTemplate_ThenTheNumberOfModifiedEntriesIsOne()
		{
			// Arrange
			var blogUser = new BlogUser { UserId = 1L, GroupName = "Group 1" };
			var blogUser2 = new BlogUser { UserId = 1L, GroupName = "Group 2" };
			await blogUserRepo.CreateManyAsync(blogUser, blogUser2);
			var updateBlogUserMutationTemplate = GetUpdateSingleBlogUserMutationTemplate(blogUser.UserId, blogUser.GroupName);
			// Act
			var numberOfEntriesModified = await updateBlogUserMutationTemplate.MutateAsync();
			// Assert
			Assert.Equal(1, numberOfEntriesModified);
		}

		[Fact]
		public async Task GivenAFewBlogUsers_WhenWeExecuteAnUpdateSingleMutationTemplate_ThenTheCorrectOneIsUpdated()
		{
			// Arrange
			await blogUserRepo.CreateManyAsync(blogUser, blogUser2);
			var updateBlogUserMutationTemplate = GetUpdateSingleBlogUserMutationTemplate(blogUser.UserId, blogUser.GroupName);
			// Act
			await updateBlogUserMutationTemplate.MutateAsync();
			// Assert
			var retrievedBlogUserOption = await blogUserRepo.GetOneAsync(blogUser);
			var retrievedBlogUser = Assert.IsType<AtomicOption<BlogUser>.Exists>(retrievedBlogUserOption).Value;
			Assert.Equal(BlogUser.BlogUserRole.Moderator, retrievedBlogUser.UserRole);
		}

		[Fact]
		public async Task GivenNoBlogUsers_WhenWeExecuteAnUpdateMutationTemplate_ThenZeroEntriesAreModified()
		{
			// Arrange
			var updateBlogUserMutationTemplate = GetUpdateSingleBlogUserMutationTemplate(1L, "Group 1");
			// Act
			var numberOfEntriesModified = await updateBlogUserMutationTemplate.MutateAsync();
			// Assert
			Assert.Equal(0, numberOfEntriesModified);
		}

		[Fact]
		public async Task GivenAnExceptionHandlerWasProvided_WhenWeExecuteMutationThatThrowsException_ThenTheExceptionHandlerIsCalled()
		{
			// Arrange
			var updateMutationTemplateWithCustomExceptionHandler = GetMutationTemplateWithCustomExceptionHandler();
			// Act
			var numberOfEntriesModified = await updateMutationTemplateWithCustomExceptionHandler.MutateAsync();
			// Assert
			Assert.True(customExceptionHandlerWasCalled);
			Assert.Equal(0, numberOfEntriesModified);
		}

		[Fact]
		public async Task GivenACustomCancellationToken_WhenWeCancelAndExecuteMutationTemplate_ThenTaskCanceledExceptionIsThrown()
		{
			// Arrange
			var mutationTemplateWithCancellationToken = GetMutationTemplateWithCustomCancellationToken();
			// Assert
			await Assert.ThrowsAsync<TaskCanceledException>(async () =>
			{
				// Act
				customCancellationTokenSource.Cancel();
				await mutationTemplateWithCancellationToken.MutateAsync();
			});
		}

		[Fact]
		public async Task GivenAFewBlogUsers_WhenWeExecuteDeleteSingleMutationTemplateWithParameters_ThenOnlyThatSpecificBlogUserIsDeleted()
		{
			// Arrange
			await blogUserRepo.CreateManyAsync(blogUser, blogUser2, blogUser3);
			var deleteBlogUserMutationTemplate = GetDeleteBlogUserMutationTemplate();
			// Act
			var numberOfEntriedDeleted = await deleteBlogUserMutationTemplate.MutateAsync(new { blogUser3.UserId });
			// Assert
			Assert.Equal(1, numberOfEntriedDeleted);
		}

		[Fact]
		public async Task GivenAFewBlogPosts_WhenWeExecuteMutationTemplateThatUpdatesASingleBlogPostsGenre_ThenOneBlogPostsIsUpdated()
		{
			// Arrange
			await blogPostRepo.CreateManyAsync(blogPost, blogPost2);
			var updateSingleBlogPostGenreMutationTemplate = GetUpdateSingleBlogPostGenreMutationTemplate();
			// Act
			var numberUpdated = await updateSingleBlogPostGenreMutationTemplate.MutateAsync(new { blogPost.PostId, blogPost.Genre, NewGenre = BlogPost.BlogPostGenre.Thriller });
			// Assert
			Assert.Equal(1, numberUpdated);
			var retrievedBlogPostOption = await blogPostRepo.GetOneAsync(new BlogPost { PostId = blogPost.PostId, Genre = BlogPost.BlogPostGenre.Thriller });
			var retrievedBlogPost = Assert.IsType<AtomicOption<BlogPost>.Exists>(retrievedBlogPostOption).Value;
			Assert.Equal(BlogPost.BlogPostGenre.Thriller, retrievedBlogPost.Genre);
		}

		[Fact]
		public async Task GivenAFewCustomerOrders_WhenWeExecuteMutationTemplateThatUpdatesFulfillmentTypeWithParameter_ThenEachCustomerOrderIsUpdated()
		{
			// Arrange
			await customerOrderRepo.CreateManyAsync(customerOrder, customerOrder2, customerOrder3);
			var updateFulfillmentTypeMutationTemplate = GetUpdateFulfillmentTypeMutationTemplate();
			// Act
			var numberUpdated = await updateFulfillmentTypeMutationTemplate.MutateAsync(new { FulfillmentType = CustomerOrder.FulfillmentTypes.Delivery });
			// Assert
			Assert.Equal(3, numberUpdated);
			var updatedCustomerOrders = await GetAllCustomerOrders();
			foreach (var updatedCustomerOrder in updatedCustomerOrders)
				Assert.Equal(CustomerOrder.FulfillmentTypes.Delivery, updatedCustomerOrder.FulfillmentType);
		}

		protected static Task CustomExceptionHandler(Exception err)
		{
			customExceptionHandlerWasCalled = true;
			return Task.CompletedTask;
		}

		protected abstract IAtomicMutationTemplate GetUpdateSingleBlogUserMutationTemplate(long userId, string groupName);
		protected abstract IAtomicRepository<T> GetAtomicRepository<T>() where T : class, new();
		protected abstract IAtomicMutationTemplate GetMutationTemplateWithCustomExceptionHandler();
		protected abstract IAtomicMutationTemplate GetMutationTemplateWithCustomCancellationToken();
		protected abstract IAtomicMutationTemplate GetDeleteBlogUserMutationTemplate();
		protected abstract IAtomicMutationTemplate GetUpdateSingleBlogPostGenreMutationTemplate();
		protected abstract IAtomicMutationTemplate GetUpdateFulfillmentTypeMutationTemplate();
		protected abstract Task<IEnumerable<CustomerOrder>> GetAllCustomerOrders();
		protected abstract void Cleanup();

		public void Dispose()
		{
			Cleanup();
			customExceptionHandlerWasCalled = false;
		}
	}
}
