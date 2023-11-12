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

		public CreateOneAsyncTests(
			IAtomicRepositoryFactory<BlogPostAuthor> authorRepoFactory,
			IAtomicRepositoryFactory<CustomerAddress> customerAddressFactory,
			string connectionString
		)
		{
			authorRepo = authorRepoFactory.CreateRepository(connectionString);
			customerAddressRepo = customerAddressFactory.CreateRepository(connectionString);
		}

		[Fact]
		public async Task WhenWeCreateABlogPostAuthor_ThenWhenWeGetItBackItExists()
		{
			var author = new BlogPostAuthor
			{
				AuthorId = 2L,
				AuthorName = "Test",
				AuthorSinceDate = DateTime.Today
			};
			// Act
			var createdAuthor = await authorRepo.CreateOneAsync(author);
			// Assert
			var retrievedAuthor = await GetExistingModelAsync(author, authorRepo);
			Assert.Equal(createdAuthor.AuthorId, retrievedAuthor.AuthorId);
		}


		[Fact]
		public async Task GivenACustomerAddressWithSomeNulls_WhenWeCreateOne_ThenItOnlyHasNonNullFieldsDefined()
		{
			// Arrange
			var customerAddress = new CustomerAddress
			{
				PhoneNumber = "+1234567890",
				City = "Sacramento",
				Province = "California"
			};
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
