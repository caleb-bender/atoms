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

		[Fact]
		public async Task GivenANullModel_WhenWeUpdateOne_ThenZeroAreUpdated()
		{
			// Arrange
			var blogPostRepo = CreateRepository<BlogPost>();
			// Act
			var numberUpdated = await blogPostRepo.UpdateOneAsync(null);
			// Assert
			Assert.Equal(0, numberUpdated);
		}

		[Fact]
		public async Task GivenABlogPostWithTooLongOfATitle_WhenWeUpdateOne_ThenAStringPropertyValueExceedsMaxLengthExceptionIsThrown()
		{
			// Arrange
			var blogPostRepo = CreateRepository<BlogPost>();
			var createdBlogPost = await blogPostRepo.CreateOneAsync(new BlogPost { PostId = 1L, Genre = BlogPost.BlogPostGenre.Horror, Title = "Scary" });
			// Assert
			await Assert.ThrowsAsync<StringPropertyValueExceedsMaxLengthException>(async () =>
			{
				createdBlogPost.Title = "0123456789012345678901234567890123456789";
				await blogPostRepo.UpdateOneAsync(createdBlogPost);
			});
		}

		[Fact]
		public async Task WhenWeAttemptToUpdateAModelDoesNotMatchEntitySchema_ThenAModelDbEntityMismatchExceptionIsThrown()
		{
			// Arrange
			var jobPostingMismatchRepo = CreateRepository<JobPostingModelEntityMismatch>();
			var jobPostMismatch = new JobPostingModelEntityMismatch { Id = Guid.NewGuid() };
			// Assert
			await Assert.ThrowsAsync<ModelDbEntityMismatchException>(async () =>
			{
				// Act
				await jobPostingMismatchRepo.UpdateOneAsync(jobPostMismatch);
			});
		}

		[Fact]
		public async Task WhenWeAttemptToUpdateAModelThatsMapsToNonexistentDatabaseEntity_ThenADbEntityNotFoundExceptionIsThrown()
		{
			// Arrange
			var nonexistentModelRepo = CreateRepository<NonexistentModel>();
			var nonexistentModel = new NonexistentModel { Id = 1 };
			// Assert
			await Assert.ThrowsAsync<DbEntityNotFoundException>(async () =>
			{
				// Act
				await nonexistentModelRepo.UpdateOneAsync(nonexistentModel);
			});
		}

		[Fact]
		public async Task WhenWeAttemptToUpdateATypeMismatchModel_ThenAModelPropertyTypeMismatchExceptionIsThrown()
		{
			// Arrange
			var typeMismatchModelRepo = CreateRepository<TypeMismatchModel>();
			var typeMismatchModel = new TypeMismatchModel { Id = Guid.NewGuid(), DateCreated = 3 };
			// Assert
			await Assert.ThrowsAsync<ModelPropertyTypeMismatchException>(async () =>
			{
				await typeMismatchModelRepo.UpdateOneAsync(typeMismatchModel);
			});
		}

		[Fact]
		public async Task GivenACreatedHolidayMatrix_WhenWeUpdateOne_ThenTheyAreUpdated()
		{
			// Arrange
			var holidayMatrixRepo = CreateRepository<HolidayMatrix>();
			var createdHolidayMatrix = await holidayMatrixRepo.CreateOneAsync(
				new HolidayMatrix {
					HolidayDay = HolidayMatrix.WeekDays.Monday,
					RouteDay = HolidayMatrix.WeekDays.Tuesday,
					DaysToSkip = 1
				}
			);
			// Act
			createdHolidayMatrix.DaysToSkip = 2;
			var numberUpdated = await holidayMatrixRepo.UpdateOneAsync(createdHolidayMatrix);
			// Assert
			Assert.Equal(1, numberUpdated);
			var updatedHolidayMatrix = await GetUpdatedModel(createdHolidayMatrix, holidayMatrixRepo);
			Assert.Equal(2, updatedHolidayMatrix.DaysToSkip);
		}

		[Fact]
		public async Task GivenANullModel_WhenWeUpdateMany_ThenAnArgumentNullExceptionIsThrown()
		{
			// Arrange
			var blogPostRepo = CreateRepository<BlogPost>();
			// Assert
			await Assert.ThrowsAsync<ArgumentNullException>(async () =>
			{
				await blogPostRepo.UpdateManyAsync(new[] {
					new BlogPost { PostId = 2L, Genre = BlogPost.BlogPostGenre.Horror },
					null
				});
			});
		}

		[Fact]
		public async Task GivenARepositoryCreatedWithATableName_WhenWeUpdateAnAnonymousModel_ThenItIsUpdatedSuccessfully()
		{
			// Arrange
			var employeeRepo = CreateRepository<EmployeeAnonymous>("Employees");
            var employee = new EmployeeAnonymous { EmployeeId = Guid.NewGuid(), Salary = 102_000M };
            await employeeRepo.CreateOneAsync(employee);
			// Act
			employee.Salary = 0M;
			await employeeRepo.UpdateOneAsync(employee);
            // Assert
            var retrievedEmployee = await GetUpdatedModel(employee, employeeRepo);
            Assert.Equal(employee.EmployeeId, retrievedEmployee.EmployeeId);
            Assert.Equal(0M, retrievedEmployee.Salary);
        }

        [Fact]
		public async Task GivenARepositoryWithCreatedTimeDatas_WhenATimeDataIsUpdated_And_Retrieved_ThenItIsCorrect()
		{
            // Arrange
            var timeDataRepo = CreateRepository<TimeData>();
            var timeData = new TimeData { Time = new TimeOnly(12, 0, 0), TimeSpan = new TimeSpan(3, 0, 0) };
            timeData = await timeDataRepo.CreateOneAsync(timeData);
            // Act
            timeData.Time = new TimeOnly(13, 0, 0);
			timeData.TimeSpan = new TimeSpan(4, 0, 0);
            await timeDataRepo.UpdateOneAsync(timeData);
            // Assert
            var retrievedTimeData = await GetUpdatedModel(timeData, timeDataRepo);
            Assert.Equal(timeData.Time, retrievedTimeData.Time);
            Assert.Equal(timeData.TimeSpan, retrievedTimeData.TimeSpan);
        }

        private async Task<T> GetUpdatedModel<T>(T model, IAtomicRepository<T> repo) where T : class, new()
		{
			var atomicOption = await repo.GetOneAsync(model);
			if (atomicOption is AtomicOption<T>.Exists modelExists)
				return modelExists.Value;
			else
				throw new ArgumentNullException("The model doesn't exist");
		}

		protected abstract IAtomicRepository<T> CreateRepository<T>(string? tableName = null) where T: class, new();
		protected abstract void Cleanup();

		public void Dispose()
		{
			Cleanup();
		}
	}
}
