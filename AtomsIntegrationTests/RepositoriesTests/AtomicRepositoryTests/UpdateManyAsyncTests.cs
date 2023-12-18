using AtomsIntegrationTests.Models;
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
