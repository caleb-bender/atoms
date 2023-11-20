using CalebBender.Atoms.Exceptions;
using CalebBender.Atoms.Repositories;
using CalebBender.Atoms.Templates.Query;
using AtomsIntegrationTests.Models;

namespace AtomsIntegrationTests.TemplateTests.QueryTemplateTests
{
	public abstract class AtomicQueryTemplateMappingErrorsTests : IDisposable
	{
		IAtomicQueryTemplate<NonexistentModel> nonexistentModelTemplate;
		IAtomicQueryTemplate<JobPostingModelEntityMismatch> jobPostingModelEntityMismatchTemplate;
		IAtomicQueryTemplate<TypeMismatchModel3> typeMismatchModelTemplate;

		protected AtomicQueryTemplateMappingErrorsTests(
			IAtomicQueryTemplate<NonexistentModel> nonexistentModelTemplate,
			IAtomicQueryTemplate<JobPostingModelEntityMismatch> jobPostingModelEntityMismatchTemplate,
			IAtomicQueryTemplate<TypeMismatchModel3> typeMismatchModelTemplate)
		{
			this.nonexistentModelTemplate = nonexistentModelTemplate;
			this.jobPostingModelEntityMismatchTemplate = jobPostingModelEntityMismatchTemplate;
			this.typeMismatchModelTemplate = typeMismatchModelTemplate;
		}


		[Fact]
		public async Task GivenAModelThatDoesNotMapToADatabaseEntity_WhenWeLazilyQuery_ThenADbEntityNotFoundExceptionIsThrown()
		{
			// Assert
			await Assert.ThrowsAsync<DbEntityNotFoundException>(async () =>
			{
				// Act
				var lazyData = nonexistentModelTemplate.QueryLazy();
				await foreach (var data in lazyData) { }
			});
		}

		[Fact]
		public async Task GivenAModelThatHasAPropertyThatDoesNotMapToADatabaseProperty_WhenWeLazilyQuery_ThenAModelDbEntityMismatchExceptionIsThrown()
		{
			// Arrange
			var jobPostingRepo = GetAtomicRepository<JobPosting>();
			await jobPostingRepo.CreateOneAsync(new JobPosting { PostingId = 1L, EmployerId = 1L });
			// Assert
			await Assert.ThrowsAsync<ModelDbEntityMismatchException>(async () =>
			{
				// Act
				var lazyData = jobPostingModelEntityMismatchTemplate.QueryLazy();
				await foreach (var data in lazyData) { }
			});
		}

		[Fact]
		public async Task GivenAModelWithAPropertyTypeMismatchWithDatabaseProperty_WhenWeLazilyQuery_ThenAModelPropertyTypeMismatchExceptionIsThrown()
		{
			// Arrange
			var typeMismatchRepo = GetAtomicRepository<TypeMismatchModel>();
			await typeMismatchRepo.CreateOneAsync(new TypeMismatchModel { Id = Guid.NewGuid() });
			// TypeMismatchModel3 has unique id of type long
			// but maps to TypeMismatchModels with unique id type of Guid
			// Assert
			await Assert.ThrowsAsync<ModelPropertyTypeMismatchException>(async () =>
			{
				var lazyData = typeMismatchModelTemplate.QueryLazy();
				await foreach (var data in lazyData) { }
			});
		}

		public void Dispose()
		{
			Cleanup();
		}

		protected abstract void Cleanup();
		protected abstract IAtomicRepository<T> GetAtomicRepository<T>() where T : class, new();
	}
}
