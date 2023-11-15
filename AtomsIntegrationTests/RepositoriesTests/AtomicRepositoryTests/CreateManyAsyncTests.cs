using Atoms.Exceptions;
using Atoms.Repositories;
using Atoms.Repositories.Factories;
using Atoms.Utils;
using AtomsIntegrationTests.Models;

namespace AtomsIntegrationTests.RepositoriesTests.AtomicRepositoryTests
{
	public abstract class CreateManyAsyncTests : IDisposable
	{
		private readonly IAtomicRepository<BlogPostAuthor> authorRepo;
		private readonly IAtomicRepository<CustomerAddress> customerAddressRepo;
		private readonly IAtomicRepository<JobPosting> jobPostingRepo;
		private readonly IAtomicRepository<Employee> employeeRepo;
		private readonly IAtomicRepository<BlogUser> blogUserRepo;
		private readonly IAtomicRepository<ModelWithIgnored> modelWithIgnoredRepo;
		private readonly IAtomicRepository<JobPostingModelEntityMismatch> jobPostingMismatchRepo;
		private readonly IAtomicRepository<NonexistentModel> nonexistentModelRepo;
		private readonly IAtomicRepository<TypeMismatchModel> typeMismatchModelRepo;
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

		private readonly Employee employee = new Employee
		{
			EmployeeId = Guid.NewGuid(),
			Salary = 120_000.00M
		};

		private readonly Employee employee2 = new Employee
		{
			EmployeeId = Guid.NewGuid(),
			Salary = 70_000.00M
		};

		private readonly Employee employee3 = new Employee
		{
			EmployeeId = Guid.NewGuid(),
			Salary = 95_000.00M
		};

		private readonly BlogUser blogUser1 = new BlogUser
		{
			UserId = 123L,
			GroupName = "Group 1"
		};

		private readonly BlogUser blogUser2 = new BlogUser
		{
			UserId = 456L,
			GroupName = "Group 2"
		};

		private readonly JobPostingModelEntityMismatch jobPostMismatch = new JobPostingModelEntityMismatch
		{
			Id = Guid.NewGuid()
		};

		private readonly NonexistentModel nonexistentModel = new NonexistentModel
		{
			Id = 1
		};

		public CreateManyAsyncTests(
			IAtomicRepositoryFactory<BlogPostAuthor> authorRepoFactory,
			IAtomicRepositoryFactory<CustomerAddress> customerAddressRepoFactory,
			IAtomicRepositoryFactory<JobPosting> jobPostingRepoFactory,
			IAtomicRepositoryFactory<Employee> employeeRepoFactory,
			IAtomicRepositoryFactory<BlogUser> blogUserRepoFactory,
			IAtomicRepositoryFactory<ModelWithIgnored> modelWithIgnoredRepoFactory,
			IAtomicRepositoryFactory<JobPostingModelEntityMismatch> jobPostingMismatchRepoFactory,
			IAtomicRepositoryFactory<NonexistentModel> nonexistentModelRepoFactory,
			IAtomicRepositoryFactory<TypeMismatchModel> typeMismatchModelRepoFactory,
			string connectionString
		)
		{
			authorRepo = authorRepoFactory.CreateRepository(connectionString);
			customerAddressRepo = customerAddressRepoFactory.CreateRepository(connectionString);
			jobPostingRepo = jobPostingRepoFactory.CreateRepository(connectionString);
			employeeRepo = employeeRepoFactory.CreateRepository(connectionString);
			blogUserRepo = blogUserRepoFactory.CreateRepository(connectionString);
			modelWithIgnoredRepo = modelWithIgnoredRepoFactory.CreateRepository(connectionString);
			jobPostingMismatchRepo = jobPostingMismatchRepoFactory.CreateRepository(connectionString);
			nonexistentModelRepo = nonexistentModelRepoFactory.CreateRepository(connectionString);
			typeMismatchModelRepo = typeMismatchModelRepoFactory.CreateRepository(connectionString);
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
			// Assert
			await Assert.ThrowsAsync<DuplicateUniqueIdException>(async () =>
			{
				// Act
				await jobPostingRepo.CreateManyAsync(new List<JobPosting> { jobPosting, jobPosting });
			});
		}

		[Fact]
		public async Task WhenWeCreateEmployeeWithAutoGeneratedUniqueIdProperty_ThenWeGetItBackWhenWeRetrieveIt()
		{
			// Act
			var createdEmployee = await employeeRepo.CreateOneAsync(employee);
			// Assert
			var retrievedEmployee = await GetExistingModelAsync(createdEmployee, employeeRepo);
			Assert.Equal(employee.Salary, retrievedEmployee.Salary);
		}

		[Fact]
		public async Task WhenWeCreateMultipleEmployees_ThenWeGetBackEachEmployeeWithLocationIdSet()
		{
			// Act
			var createdEmployees = await employeeRepo.CreateManyAsync(employee, employee2, employee3);
			foreach (var createdEmployee in createdEmployees)
				// Assert
				Assert.NotEqual(-1L, createdEmployee.LocationId);
		}

		[Fact]
		public async Task WhenWeAttemptToCreateANullOrEmptyIEnumerableOfModels_ThenAnEmptyIEnumerableIsReturned()
		{
			// Act
			var createdEmployees = await employeeRepo.CreateManyAsync(new List<Employee>());
			// Assert
			Assert.Empty(createdEmployees);
			// Act
			createdEmployees = await employeeRepo.CreateManyAsync(null);
			// Assert
			Assert.Empty(createdEmployees);
		}

		[Fact]
		public async Task WhenWeCreateMultipleBlogUsersWithDifferentDatabaseEntityAndPropertyNames_ThenTheyAreRetrievedAndCorrect()
		{
			// Act
			var createdBlogUsers = await blogUserRepo.CreateManyAsync(blogUser1, blogUser2);
			// Assert
			var retrievedBlogUser1 =
				await GetExistingModelAsync(createdBlogUsers.ElementAt(0), blogUserRepo);
			var retrievedBlogUser2 =
				await GetExistingModelAsync(createdBlogUsers.ElementAt(1), blogUserRepo);
			Assert.Equal(blogUser1.UserId, retrievedBlogUser1.UserId);
			Assert.Equal(blogUser2.UserId, retrievedBlogUser2.UserId);
		}

		[Fact]
		public async Task WhenWeCreateModelWithIgnoredProperties_ThenRetrievedModelHasCorrectValuesForIgnored()
		{
			// Act
			var createdModel = await modelWithIgnoredRepo.CreateOneAsync(new ModelWithIgnored { Id = 1L });
			// Assert
			var retrievedModel = await GetExistingModelAsync(createdModel, modelWithIgnoredRepo);
			// This one is read from so will be null
			Assert.Null(retrievedModel.PropertyReadFromButNotWrittenTo);
			// This one is not read from so will be default
			Assert.Equal("default", retrievedModel.PropertyNeitherReadFromNorWrittenTo);
		}

		[Fact]
		public async Task WhenWeAttemptToCreateAModelDoesNotMatchEntitySchema_ThenAModelDbEntityMismatchExceptionIsThrown()
		{
			// Assert
			await Assert.ThrowsAsync<ModelDbEntityMismatchException>(async () =>
			{
				await jobPostingMismatchRepo.CreateOneAsync(jobPostMismatch);
			});
		}

		[Fact]
		public async Task WhenWeAttemptToCreateAModelThatsMapsToNonexistentDatabaseEntity_ThenADbEntityNotFoundExceptionIsThrown()
		{
			// Assert
			await Assert.ThrowsAsync<DbEntityNotFoundException>(async () =>
			{
				await nonexistentModelRepo.CreateOneAsync(nonexistentModel);
			});
		}

		[Fact]
		public async Task WhenWeAttemptToCreateATypeMismatchModel_ThenAModelPropertyTypeMismatchExceptionIsThrown()
		{
			// Arrange
			var typeMismatchModel = new TypeMismatchModel { Id = Guid.NewGuid(), DateCreated = 3 };
			// Assert
			await Assert.ThrowsAsync<ModelPropertyTypeMismatchException>(async () =>
			{
				await typeMismatchModelRepo.CreateOneAsync(typeMismatchModel);
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
