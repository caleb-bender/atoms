using Atoms.Repositories;
using Atoms.Templates.Query;
using Atoms.Utils;
using AtomsIntegrationTests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtomsIntegrationTests.TemplateTests.QueryTemplateTests
{
	public abstract class AtomicQueryTemplateModelTests : IDisposable
	{
		[Fact]
		public async Task GivenThereAreNoBlogPostAuthors_WhenWeLazilyQueryBlogPostAuthors_ThenAnEmptyAsyncEnumerableIsReturned()
		{
			// Arrange
			var authorQueryTemplate = GetAtomicQueryTemplate<BlogPostAuthor>();
			// Act
			var lazyAuthors = authorQueryTemplate.QueryLazy();
			// Assert
			int numberOfResults = 0;
			await foreach (var author in lazyAuthors)
				numberOfResults++;
			Assert.Equal(0, numberOfResults);
		}

		public void Dispose()
		{
			Cleanup();
		}
		protected abstract void Cleanup();
		protected abstract IAtomicQueryTemplate<T> GetAtomicQueryTemplate<T>() where T : class, new();
		protected abstract IAtomicRepository<T> GetAtomicRepository<T>() where T: class, new();
	}
}
