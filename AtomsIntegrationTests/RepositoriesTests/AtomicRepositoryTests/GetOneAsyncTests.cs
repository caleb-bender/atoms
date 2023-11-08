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
using static AtomsIntegrationTests.Models.BlogUser;

namespace AtomsIntegrationTests.RepositoriesTests.AtomicRepositoryTests
{
	public abstract class GetOneAsyncTests : IDisposable
	{
		private readonly IAtomicRepository<BlogPostAuthor> authorRepo;
		private readonly IAtomicRepository<TypeMismatchModel> typeMismatchRepo;
		private readonly IAtomicRepository<BlogUser> blogUserRepo;

		public GetOneAsyncTests(
			IAtomicRepositoryFactory<BlogPostAuthor> authorRepoFactory,
			IAtomicRepositoryFactory<TypeMismatchModel> typeMismatchRepoFactory,
			IAtomicRepositoryFactory<BlogUser> blogUserRepoFactory,
			string connectionString
		)
		{
			authorRepo = authorRepoFactory.CreateRepository(connectionString);
			typeMismatchRepo = typeMismatchRepoFactory.CreateRepository(connectionString);
			blogUserRepo = blogUserRepoFactory.CreateRepository(connectionString);
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

		[Fact]
		public async Task GivenATypeMismatchBetweenDatabaseAndModelProperty_WhenWeAttemptToGetTheModel_ThenAPropertyTypeMismatchExceptionIsThrown()
		{
			// Arrange
			var guid = Guid.NewGuid();
			await CreateOneTypeMismatchModelAsync(guid, status: "OPEN");
			// Assert
			await Assert.ThrowsAsync<ModelPropertyTypeMismatchException>(async () =>
			{
				// TypeMismatchModel's Status property is an int and not a string, which should cause exception to be thrown
				var typeMismatchModel = await typeMismatchRepo.GetOneAsync(new TypeMismatchModel { Id = guid });
			});
		}

		[Fact]
		public async Task GivenModelWithDifferentNameFromDatabase_WhenWeAttemptToGetTheModel_ThenTheCorrectOneIsRetrieved()
		{
			// Arrange
			await CreateOneBlogUserAsync(1L, "Group 1");
			await CreateOneBlogUserAsync(1L, "Group 2");
			await CreateOneBlogUserAsync(1L, "Group 3");
			// Act
			var blogUserOption = await blogUserRepo.GetOneAsync(new BlogUser { UserId = 1L, GroupName = "Group 2" });
			// Assert
			var blogUserExists = Assert.IsType<AtomicOption<BlogUser>.Exists>(blogUserOption);
			AssertThatBlogUserIsCorrect(blogUserExists, 1L, "Group 2");
		}

		[Fact]
		public async Task GivenModelUniqueIdPropertyWithDifferentNameFromDatabase_WhenWeAttemptToGetTheModel_ThenTheCorrectOneIsRetrieved()
		{
			// Arrange
			await CreateOneBlogUserAsync(1L, "Group 1");
			await CreateOneBlogUserAsync(2L, "Group 1");
			await CreateOneBlogUserAsync(2L, "Group 2");
			// Act
			var blogUserOption = await blogUserRepo.GetOneAsync(new BlogUser { UserId = 2L, GroupName = "Group 2" });
			// Assert
			var blogUserExists = Assert.IsType<AtomicOption<BlogUser>.Exists>(blogUserOption);
			AssertThatBlogUserIsCorrect(blogUserExists, 2L, "Group 2");
		}

		[Fact]
		public async Task GivenBlogUserWithUserRoleDefinedAsStringInDatabase_WhenWeAttemptToGetTheModel_ThenTheCorrectOneIsRetrieved()
		{
			// Arrange
			await CreateOneBlogUserAsync(1L, "Group 1");
			await CreateOneBlogUserAsync(2L, "Group 1", BlogUserRole.Contributor.ToString());
			// Act
			var blogUserOption = await blogUserRepo.GetOneAsync(new BlogUser { UserId = 2L, GroupName = "Group 1" });
			// Assert
			var blogUserExists = Assert.IsType<AtomicOption<BlogUser>.Exists>(blogUserOption);
			AssertThatBlogUserIsCorrect(blogUserExists, 2L, "Group 1", BlogUserRole.Contributor);
		}

		[Fact]
		public async Task GivenABlogUserWithAUserRoleStringThatCantBeMappedToEnum_WhenWeAttemptToGetModel_ThenAEnumPropertyMappingFailedExceptionIsThrown()
		{
			// Arrange
			await CreateOneBlogUserAsync(1L, "Group 1", "Invalid");
			// Assert
			await Assert.ThrowsAsync<EnumPropertyMappingFailedException>(async () =>
			{
				// Act
				var blogUserOption = await blogUserRepo.GetOneAsync(new BlogUser { UserId = 1L, GroupName = "Group 1" });
			});
		}

		[Fact]
		public async Task GivenABlogUserWithExtraPropertyNotInModel_WhenWeAttemptToGetModel_ThenAllOtherPropertiesAreCorrect()
		{
			// Arrange
			await CreateOneBlogUserAsync(3L, "Group 1");
			// Act
			var blogUserOption = await blogUserRepo.GetOneAsync(new BlogUser { UserId = 3L, GroupName = "Group 1" });
			// Assert
			var blogUserExists = Assert.IsType<AtomicOption<BlogUser>.Exists>(blogUserOption);
			AssertThatBlogUserIsCorrect(blogUserExists, 3L, "Group 1");
		}

		private static void AssertThatBlogUserIsCorrect(AtomicOption<BlogUser>.Exists blogUserExists, long expectedUserId, string expectedGroupName, BlogUserRole expectedUserRole = BlogUserRole.Reader)
		{
			var blogUser = blogUserExists.Value;
			Assert.Equal(expectedUserId, blogUser.UserId);
			Assert.Equal(expectedGroupName, blogUser.GroupName);
			Assert.Equal(expectedUserRole, blogUser.UserRole);
		}

		public void Dispose()
		{
			Cleanup();
		}

		protected abstract void Cleanup();
		protected abstract Task CreateOneBlogPostAuthorAsync(long authorId, string authorName, DateTime authorSinceDate);
		protected abstract Task CreateOneTypeMismatchModelAsync(Guid id, string status);
		protected abstract Task CreateOneBlogUserAsync(long userId, string groupName, string? userRole = null);
	}
}
