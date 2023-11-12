using Atoms.Exceptions;
using Atoms.Repositories;
using Atoms.Repositories.Factories;
using Atoms.Utils;
using AtomsIntegrationTests.Models;

namespace AtomsIntegrationTests.RepositoriesTests.AtomicRepositoryTests
{
	public abstract class CreateOneAsyncTests : IDisposable
	{
		private readonly IAtomicRepository<BlogPostAuthor> authorRepo;
		private readonly IAtomicRepository<CustomerAddress> customerAddressRepo;
		private readonly IAtomicRepository<JobPosting> jobPostingRepo;
		private readonly CustomerAddress customerAddress = new CustomerAddress
		{
			PhoneNumber = "+1234567890",
			City = "Sacramento",
			Province = "California"
		};

		private readonly BlogPostAuthor author = new BlogPostAuthor
		{
			AuthorId = 2L,
			AuthorName = "Test",
			AuthorSinceDate = DateTime.Today
		};

		private readonly JobPosting jobPosting = new JobPosting
		{
			PostingId = 456L,
			EmployerId = 321L
		};

		public CreateOneAsyncTests(
			IAtomicRepositoryFactory<BlogPostAuthor> authorRepoFactory,
			IAtomicRepositoryFactory<CustomerAddress> customerAddressRepoFactory,
			IAtomicRepositoryFactory<JobPosting> jobPostingRepoFactory,
			string connectionString
		)
		{
			authorRepo = authorRepoFactory.CreateRepository(connectionString);
			customerAddressRepo = customerAddressRepoFactory.CreateRepository(connectionString);
			jobPostingRepo = jobPostingRepoFactory.CreateRepository(connectionString);
		}

		[Fact]
		public async Task WhenWeCreateABlogPostAuthor_ThenWhenWeGetItBackItExists()
		{
			// Act
			var createdAuthor = await authorRepo.CreateOneAsync(author);
			// Assert
			var retrievedAuthor = await GetExistingModelAsync(author, authorRepo);
			Assert.Equal(createdAuthor.AuthorId, retrievedAuthor.AuthorId);
		}


		[Fact]
		public async Task GivenACustomerAddressWithSomeNulls_WhenWeCreateOne_ThenItOnlyHasNonNullFieldsDefined()
		{
			// Act
			var createdCustomerAddress = await customerAddressRepo.CreateOneAsync(customerAddress);
			// Assert
			var retrievedCustomerAddress = await GetExistingModelAsync(createdCustomerAddress, customerAddressRepo);
			Assert.Equal(customerAddress.PhoneNumber, retrievedCustomerAddress.PhoneNumber);
			Assert.Equal(customerAddress.City, retrievedCustomerAddress.City);
			Assert.Equal(customerAddress.Province, retrievedCustomerAddress.Province);
			Assert.Null(retrievedCustomerAddress.UnitNumber);
			Assert.Null(retrievedCustomerAddress.StreetNumber);
			Assert.Null(retrievedCustomerAddress.Street);
			Assert.Null(retrievedCustomerAddress.PostalCode);
			Assert.Null(retrievedCustomerAddress.Country);
		}

		[Fact]
		public async Task GivenABlogPostAuthorAlreadyExists_WhenWeAttemptToCreateTheSameOne_ThenADuplicateUniqueIdExceptionIsThrown()
		{
			// Arrange
			await authorRepo.CreateOneAsync(author);
			// Assert
			await Assert.ThrowsAsync<DuplicateUniqueIdException>(async () =>
			{
				// Act
				await authorRepo.CreateOneAsync(author);
			});
		}

		[Fact]
		public async Task GivenAJobPostingAlreadyExists_WhenWeAttemptToCreateTheSameOne_ThenADuplicateUniqueIdExceptionIsThrown()
		{
			// Arrange
			await jobPostingRepo.CreateOneAsync(jobPosting);
			// Assert
			await Assert.ThrowsAsync<DuplicateUniqueIdException>(async () =>
			{
				// Act
				await jobPostingRepo.CreateOneAsync(jobPosting);
			});
		}


		private async Task<TModel> GetExistingModelAsync<TModel>(TModel model, IAtomicRepository<TModel> repo)
			where TModel : class, new()
		{
			var retrievedModelOption = await repo.GetOneAsync(model);
			var retrievedModelExists = Assert.IsType<AtomicOption<TModel>.Exists>(retrievedModelOption);
			var retrievedModel = retrievedModelExists.Value;
			return retrievedModel;
		}

		public void Dispose()
		{
			Cleanup();
		}
		protected abstract void Cleanup();
	}
}
