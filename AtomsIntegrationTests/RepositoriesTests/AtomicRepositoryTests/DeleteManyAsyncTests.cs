using CalebBender.Atoms.Exceptions;
using CalebBender.Atoms.Repositories;
using CalebBender.Atoms.Repositories.Factories;
using CalebBender.Atoms.Utils;
using AtomsIntegrationTests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtomsIntegrationTests.RepositoriesTests.AtomicRepositoryTests
{
	public abstract class DeleteManyAsyncTests : IDisposable
	{
		private readonly IAtomicRepository<CustomerAddress> customerAddressRepo;
		private readonly IAtomicRepository<BlogUser> blogUserRepo;
		private readonly IAtomicRepository<BlogPost> blogPostRepo;
		private readonly IAtomicRepository<NonexistentModel> nonexistentModelRepo;
		private readonly IAtomicRepository<JobPostingModelEntityMismatch> jobPostingMismatchRepo;
		private readonly IAtomicRepository<TypeMismatchModel3> typeMismatchModelRepo;

		public DeleteManyAsyncTests(
			IAtomicRepositoryFactory<CustomerAddress> customerAddressRepoFactory,
			IAtomicRepositoryFactory<BlogUser> blogUserRepoFactory,
			IAtomicRepositoryFactory<BlogPost> blogPostRepoFactory,
			IAtomicRepositoryFactory<NonexistentModel> nonexistentModelRepoFactory,
			IAtomicRepositoryFactory<JobPostingModelEntityMismatch> jobPostingMismatchRepoFactory,
			IAtomicRepositoryFactory<TypeMismatchModel3> typeMismatchModelRepoFactory,
			string connectionString
		)
		{
			customerAddressRepo = customerAddressRepoFactory.CreateRepository(connectionString);
			blogUserRepo = blogUserRepoFactory.CreateRepository(connectionString);
			blogPostRepo = blogPostRepoFactory.CreateRepository(connectionString);
			nonexistentModelRepo = nonexistentModelRepoFactory.CreateRepository(connectionString);
			jobPostingMismatchRepo = jobPostingMismatchRepoFactory.CreateRepository(connectionString);
			typeMismatchModelRepo = typeMismatchModelRepoFactory.CreateRepository(connectionString);
		}

		[Fact]
		public async Task GivenANullOrEmptyIEnumerable_WhenWeDeleteMany_ThenNoExceptionsAreRaised()
		{
			// Act
			await customerAddressRepo.DeleteManyAsync(new List<CustomerAddress>());
			// Act
			await customerAddressRepo.DeleteManyAsync(null);
		}

		[Fact]
		public async Task GivenAFewCreatedModelsWithOneUniqueIdProperty_WhenWeDeleteMany_OnlyThoseModelsAreDeleted()
		{
			// Arrange
			var customerAddress1 = new CustomerAddress { PhoneNumber = "1234567890" };
			var customerAddress2 = new CustomerAddress { PhoneNumber = "9012345678" };
			var customerAddress3 = new CustomerAddress { PhoneNumber = "7890654312" };
			var createdCustomerAddresses = await customerAddressRepo.CreateManyAsync(customerAddress1, customerAddress2, customerAddress3);
			// Act
			await customerAddressRepo.DeleteManyAsync(customerAddress1, customerAddress2);
			// Assert
			var retrievedCustomerAddress1 = await customerAddressRepo.GetOneAsync(createdCustomerAddresses.ElementAt(0));
			var retrievedCustomerAddress2 = await customerAddressRepo.GetOneAsync(createdCustomerAddresses.ElementAt(1));
			var retrievedCustomerAddress3 = await customerAddressRepo.GetOneAsync(createdCustomerAddresses.ElementAt(2));

			Assert.IsType<AtomicOption<CustomerAddress>.Empty>(retrievedCustomerAddress1);
			Assert.IsType<AtomicOption<CustomerAddress>.Empty>(retrievedCustomerAddress2);
			// This one wasn't deleted
			Assert.IsType<AtomicOption<CustomerAddress>.Exists>(retrievedCustomerAddress3);
		}

		[Fact]
		public async Task GivenAFewCreatedModelsWithMultipleUniqueIdProperties_WhenWeDeleteMany_OnlyThoseModelsAreDeleted()
		{
			// Arrange
			var blogUser1 = new BlogUser { UserId = 1L, GroupName = "Group1" };
			var blogUser2 = new BlogUser { UserId = 1L, GroupName = "Group2" };
			var blogUser3 = new BlogUser { UserId = 2L, GroupName = "Group3" };
			var createdBlogUsers = await blogUserRepo.CreateManyAsync(blogUser1, blogUser2, blogUser3);
			// Act
			await blogUserRepo.DeleteManyAsync(blogUser1, blogUser2);
			// Assert
			var retrievedBlogUser1 = await blogUserRepo.GetOneAsync(createdBlogUsers.ElementAt(0));
			var retrievedBlogUser2 = await blogUserRepo.GetOneAsync(createdBlogUsers.ElementAt(1));
			var retrievedBlogUser3 = await blogUserRepo.GetOneAsync(createdBlogUsers.ElementAt(2));

			Assert.IsType<AtomicOption<BlogUser>.Empty>(retrievedBlogUser1);
			Assert.IsType<AtomicOption<BlogUser>.Empty>(retrievedBlogUser2);
			// This one wasn't deleted
			Assert.IsType<AtomicOption<BlogUser>.Exists>(retrievedBlogUser3);
		}

		[Fact]
		public async Task GivenACreatedModelWithAnEnumUniqueIdProperty_WhenWeDeleteOne_ThenItShouldBeDeleted()
		{
			// Arrange
			var blogPost = new BlogPost { PostId = 1L, Genre = BlogPost.BlogPostGenre.Fantasy };
			var createdBlogPost = await blogPostRepo.CreateOneAsync(blogPost);
			// Act
			await blogPostRepo.DeleteOneAsync(createdBlogPost);
			// Assert
			var retrievedBlogPostOption = await blogPostRepo.GetOneAsync(createdBlogPost);
			Assert.IsType<AtomicOption<BlogPost>.Empty>(retrievedBlogPostOption);
		}

		[Fact]
		public async Task GivenAModelThatDoesNotMapToADatabaseEntity_WhenWeDeleteOne_ThenADbEntityNotFoundExceptionIsThrown()
		{
			// Arrange
			var nonexistentModel = new NonexistentModel { Id = 1 };
			// Assert
			await Assert.ThrowsAsync<DbEntityNotFoundException>(async () =>
			{
				await nonexistentModelRepo.DeleteOneAsync(nonexistentModel);
			});
		}

		[Fact]
		public async Task GivenAModelThatDoesNotMatchDatabaseEntitySchema_WhenWeDeleteOne_ThenAModelDbEntityMismatchExceptionIsThrown()
		{
			// Arrange
			var jobPostingMismatchModel = new JobPostingModelEntityMismatch { Id = Guid.Empty };
			// Assert
			await Assert.ThrowsAsync<ModelDbEntityMismatchException>(async () =>
			{
				await jobPostingMismatchRepo.DeleteOneAsync(jobPostingMismatchModel);
			});
		}

		[Fact]
		public async Task GivenAModelThatHasAUniqueIdPropertyTypeMismatchWithDatabaseProperty_WhenWeDeleteOne_ThenAModelPropertyTypeMismatchExceptionIsThrown()
		{
			// Arrange
			// TypeMismatchModel3 has unique id of type long
			// but maps to TypeMismatchModels with unique id type of Guid
			var typeMismatchModel3 = new TypeMismatchModel3 { Id = 2L };
			// Assert
			await Assert.ThrowsAsync<ModelPropertyTypeMismatchException>(async () =>
			{
				await typeMismatchModelRepo.DeleteOneAsync(typeMismatchModel3);
			});
		}

		[Fact]
		public async Task GivenThreeCreatedBlogUsers_WhenWeDeleteTwo_ThenTwoIsReturnedFromDeleteMany()
		{
			// Arrange
			var blogUser1 = new BlogUser { UserId = 1L, GroupName = "Group1" };
			var blogUser2 = new BlogUser { UserId = 1L, GroupName = "Group2" };
			var blogUser3 = new BlogUser { UserId = 2L, GroupName = "Group3" };
			await blogUserRepo.CreateManyAsync(blogUser1, blogUser2, blogUser3);
			// Act
			var numberOfBlogUsersDeleted = await blogUserRepo.DeleteManyAsync(blogUser1, blogUser3);
			// Assert
			Assert.Equal(2, numberOfBlogUsersDeleted);
		}

		[Fact]
		public async Task GivenNoModelsToDelete_WhenWeCallDeleteMany_ThenZeroIsReturned()
		{
			// Act
			var numberOfBlogUsersDeleted = await blogUserRepo.DeleteManyAsync();
			// Assert
			Assert.Equal(0, numberOfBlogUsersDeleted);
		}

		public void Dispose()
		{
			Cleanup();
		}

		protected abstract void Cleanup();
	}
}
