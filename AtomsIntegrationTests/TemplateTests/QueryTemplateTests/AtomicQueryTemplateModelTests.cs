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

		[Fact]
		public async Task GivenSomeBlogPostAuthorsExist_WhenWeLazilyQueryBlogPostAuthors_ThenTheyAreRetrieved()
		{
			// Arrange
			var blogPostAuthors = new[]
			{
				new BlogPostAuthor { AuthorId = 1L, AuthorName = "Bob Doe", AuthorSinceDate = DateTime.Today },
				new BlogPostAuthor { AuthorId = 2L, AuthorName = "Jane Smith", AuthorSinceDate = DateTime.Today }
			};
			var blogPostAuthorRepo = GetAtomicRepository<BlogPostAuthor>();
			await blogPostAuthorRepo.CreateManyAsync(blogPostAuthors);
			var blogPostAuthorTemplate = GetAtomicQueryTemplate<BlogPostAuthor>();
			// Act
			var lazyAuthors = blogPostAuthorTemplate.QueryLazy();
			// Assert
			int i = 0;
			await foreach (var author in lazyAuthors)
			{
				Assert.Equal(blogPostAuthors[i].AuthorName, author.AuthorName);
				i++;
			}
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
