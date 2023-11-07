using Atoms.Exceptions;
using Atoms.Repositories;
using Atoms.Repositories.Factories;
using Atoms.Utils;
using AtomsIntegrationTests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtomsIntegrationTests.RepositoriesTests.AtomicRepositoryTests
{
	public abstract class GetOneAsyncTests : IDisposable
	{
		private readonly IAtomicRepository<BlogPostAuthor> authorRepo;

		public GetOneAsyncTests(
			IAtomicRepositoryFactory<BlogPostAuthor> authorRepoFactory,
			string connectionString
		)
		{
			authorRepo = authorRepoFactory.CreateRepository(connectionString);
		}

		[Fact]
		public async Task GivenTheBlogPostAuthorUniqueIdIsDefaultValue_WhenWeGetOneBlogPostAuthor_ThenAnEmptyOptionalIsReturned()
		{
			// Act
			var blogPostAuthorOption = await authorRepo.GetOneAsync(new BlogPostAuthor { AuthorId = 0L });
			// Assert
			Assert.IsType<AtomicOption<BlogPostAuthor>.Empty>(blogPostAuthorOption);
		}

		[Fact]
		public async Task GivenTheBlogPostAuthorDoesNotExist_WhenWeGetOneBlogPostAuthor_ThenAnEmptyOptionalIsReturned()
		{
			// Act
			var blogPostAuthorOption = await authorRepo.GetOneAsync(new BlogPostAuthor { AuthorId = 123L });
			// Assert
			Assert.IsType<AtomicOption<BlogPostAuthor>.Empty>(blogPostAuthorOption);
		}

		[Fact]
		public async Task GivenTheBlogPostAuthorExists_WhenWeGetOneBlogPostAuthor_ThenAnExistsOptionalIsReturned()
		{
			// Arrange
			await CreateOneBlogPostAuthorAsync(1L, "Jane Smith", DateTime.Today);
			// Act
			var blogPostAuthorOption = await authorRepo.GetOneAsync(new BlogPostAuthor { AuthorId = 1L });
			// Assert
			Assert.IsType<AtomicOption<BlogPostAuthor>.Exists>(blogPostAuthorOption);
		}

		[Fact]
		public async Task GivenTheBlogPostAuthorExists_WhenWeGetOneBlogPostAuthor_TheCorrectOneIsRetrieved()
		{
			// Arrange
			await CreateOneBlogPostAuthorAsync(1L, "Bob Doe", DateTime.Today);
			await CreateOneBlogPostAuthorAsync(2L, "Frank Mills", DateTime.Today);
			// Act
			var blogPostAuthorOption = await authorRepo.GetOneAsync(new BlogPostAuthor { AuthorId = 2L });
			// Assert
			var blogPostAuthorExists = Assert.IsType<AtomicOption<BlogPostAuthor>.Exists>(blogPostAuthorOption);
			var blogPost = blogPostAuthorExists.Value;
			Assert.Equal(2L, blogPost.AuthorId);
		}

		[Fact]
		public async Task GivenTheBlogPostAuthorExists_WhenWeGetOneBlogPostAuthor_AllThePropertiesAreSetCorrectly()
		{
			// Arrange
			await CreateOneBlogPostAuthorAsync(1L, "Jane Doe", new DateTime(2023, 1, 1));
			// Act
			var blogPostAuthorOption = await authorRepo.GetOneAsync(new BlogPostAuthor { AuthorId = 1L });
			// Assert
			var blogPostAuthorExists = Assert.IsType<AtomicOption<BlogPostAuthor>.Exists>(blogPostAuthorOption);
			var blogPost = blogPostAuthorExists.Value;
			Assert.Equal(1L, blogPost.AuthorId);
			Assert.Equal("Jane Doe", blogPost.AuthorName);
			Assert.Equal(new DateTime(2023, 1, 1), blogPost.AuthorSinceDate);
		}

		public void Dispose()
		{
			Cleanup();
		}

		protected abstract void Cleanup();
		protected abstract Task CreateOneBlogPostAuthorAsync(long authorId, string authorName, DateTime authorSinceDate);
	}
}
