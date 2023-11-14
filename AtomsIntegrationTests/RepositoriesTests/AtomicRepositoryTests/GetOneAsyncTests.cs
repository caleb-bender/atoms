using Atoms.DataAttributes;
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
using static AtomsIntegrationTests.Models.BlogPost;
using static AtomsIntegrationTests.Models.BlogUser;
using static AtomsIntegrationTests.Models.CustomerOrder;

namespace AtomsIntegrationTests.RepositoriesTests.AtomicRepositoryTests
{
	public abstract class GetOneAsyncTests : IDisposable
	{
		private readonly IAtomicRepository<BlogPostAuthor> authorRepo;
		private readonly IAtomicRepository<TypeMismatchModel> typeMismatchRepo;
		private readonly IAtomicRepository<BlogUser> blogUserRepo;
		private readonly IAtomicRepository<BlogPost> blogPostRepo;
		private readonly IAtomicRepository<CustomerAddress> customerAddressRepo;
		private readonly IAtomicRepository<CustomerOrder> customerOrderRepo;
		private readonly IAtomicRepository<ModelWithIgnored> modelWithIgnoredRepo;

		public GetOneAsyncTests(
			IAtomicRepositoryFactory<BlogPostAuthor> authorRepoFactory,
			IAtomicRepositoryFactory<TypeMismatchModel> typeMismatchRepoFactory,
			IAtomicRepositoryFactory<BlogUser> blogUserRepoFactory,
			IAtomicRepositoryFactory<BlogPost> blogPostRepoFactory,
			IAtomicRepositoryFactory<CustomerAddress> customerAddressRepoFactory,
			IAtomicRepositoryFactory<CustomerOrder> customerOrderRepoFactory,
			IAtomicRepositoryFactory<ModelWithIgnored> modelWithIgnoredRepoFactory,
			string connectionString
		)
		{
			authorRepo = authorRepoFactory.CreateRepository(connectionString);
			typeMismatchRepo = typeMismatchRepoFactory.CreateRepository(connectionString);
			blogUserRepo = blogUserRepoFactory.CreateRepository(connectionString);
			blogPostRepo = blogPostRepoFactory.CreateRepository(connectionString);
			customerAddressRepo = customerAddressRepoFactory.CreateRepository(connectionString);
			customerOrderRepo = customerOrderRepoFactory.CreateRepository(connectionString);
			modelWithIgnoredRepo = modelWithIgnoredRepoFactory.CreateRepository(connectionString);
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
		public async Task GivenABlogUserInDatabaseWithExtraPropertyNotInModel_WhenWeAttemptToGetModel_ThenAllOtherPropertiesAreCorrect()
		{
			// Arrange
			await CreateOneBlogUserAsync(3L, "Group 1");
			// Act
			var blogUserOption = await blogUserRepo.GetOneAsync(new BlogUser { UserId = 3L, GroupName = "Group 1" });
			// Assert
			var blogUserExists = Assert.IsType<AtomicOption<BlogUser>.Exists>(blogUserOption);
			AssertThatBlogUserIsCorrect(blogUserExists, 3L, "Group 1");
		}

		[Fact]
		public async Task GivenACustomerAddressWithSomeNullValues_WhenWeAttemptToGetModel_ThenItsPropertiesAreCorrect()
		{
			// Arrange
			await CreateOneCustomerAddressAsync("+11234567890", "Los Angeles", "United States");
			// Act
			// Act
			var customerAddressOption = await customerAddressRepo.GetOneAsync(new CustomerAddress { PhoneNumber = "+11234567890" });
			// Assert
			var customerAddressExists = Assert.IsType<AtomicOption<CustomerAddress>.Exists>(customerAddressOption);
			var customerAddress = customerAddressExists.Value;
			Assert.Equal("+11234567890", customerAddress.PhoneNumber);
			Assert.Null(customerAddress.UnitNumber);
			Assert.Null(customerAddress.StreetNumber);
			Assert.Null(customerAddress.Street);
			Assert.Null(customerAddress.PostalCode);
			Assert.NotNull(customerAddress.City);
			Assert.Null(customerAddress.Province);
			Assert.NotNull(customerAddress.Country);
		}

		[Fact]
		public async Task GivenABlogPostWithAnEnumAsPartOfTheUniqueId_WhenWeGetModel_ThenWeGetBackCorrectOne()
		{
			// Arrange
			await CreateOneBlogPostAsync(123L, BlogPostGenre.Horror, "A Spooky Night", "Once upon a time...");
			await CreateOneBlogPostAsync(123L, BlogPostGenre.Scifi, "Amongst the Nebula", "In a desolate part of the galaxy, there was...");
			// Act
			var blogPostOption = await blogPostRepo.GetOneAsync(new BlogPost { PostId = 123L, Genre = BlogPostGenre.Scifi });
			// Assert
			var blogPostExists = Assert.IsType<AtomicOption<BlogPost>.Exists>(blogPostOption);
			var blogPost = blogPostExists.Value;
			Assert.Equal(123L, blogPost.PostId);
			Assert.Equal(BlogPostGenre.Scifi, blogPost.Genre);
			Assert.Equal("Amongst the Nebula", blogPost.Title);
			Assert.Equal("In a desolate part of the galaxy, there was...", blogPost.Content);
		}

		[Fact]
		public async Task GivenABlogPostWithMaxLengthAttributeOnTitleProperty_WhenWeGetValueThatExceedsMaxLength_ThenAPropertyValueExceedsMaxLengthExceptionIsThrown()
		{
			// Arrange
			await CreateOneBlogPostAsync(1L, BlogPostGenre.Thriller, "1234567890123456789012345678901234567890", "Content");
			// Assert
			await Assert.ThrowsAsync<PropertyValueExceedsMaxLengthException>(async () =>
			{
				var blogPostOption = await blogPostRepo.GetOneAsync(new BlogPost { PostId = 1L, Genre = BlogPostGenre.Thriller });
			});
		}

		[Fact]
		public async Task GivenABlogPostWithSerializedBlogComments_WhenWeGetAndDeserializeThoseBlogComments_ThenTheyAreCorrect()
		{
			// Arrange
			var blogComments = new List<BlogComment>
			{
				new BlogComment { Username = "john-doe", Content = "Great short story!" },
				new BlogComment { Username = "janesmith123", Content = "A decent indie tale." }
			};
			await CreateOneBlogPostAsync(1L, BlogPostGenre.Fantasy, "A Lonely Village", "Once upon a time...", blogComments);
			// Act
			var blogPostOption = await blogPostRepo.GetOneAsync(new BlogPost { PostId = 1L, Genre = BlogPostGenre.Fantasy });
			// Assert
			var blogPostExists = Assert.IsType<AtomicOption<BlogPost>.Exists>(blogPostOption);
			var blogPost = blogPostExists.Value;
			Assert.NotNull(blogPost.BlogComments);
			Assert.Equal(2, blogPost.BlogComments?.Count());
			Assert.Equal("john-doe", blogPost.BlogComments[0].Username);
		}

		[Fact]
		public async Task GivenABlogPostWithInvalidSerializedBlogComments_WhenWeAttemptToGetAndDeserialize_ThenTheBlogCommentsAreNull()
		{
			// Arrange
			await CreateOneBlogPostAsync(1L, BlogPostGenre.Thriller, "Body in the Woods", "...", null, insertInvalidBlogCommentsJson: true);
			// Act
			var blogPostOption = await blogPostRepo.GetOneAsync(new BlogPost { PostId = 1L, Genre = BlogPostGenre.Thriller });
			// Assert
			var blogPostExists = Assert.IsType<AtomicOption<BlogPost>.Exists>(blogPostOption);
			var blogPost = blogPostExists.Value;
			Assert.Null(blogPost.BlogComments);
		}

		[Theory]
		[InlineData("PC", FulfillmentTypes.PickupByCustomer)]
		[InlineData("D", FulfillmentTypes.Delivery)]
		[InlineData("PT", FulfillmentTypes.PickupByThirdParty)]
		[InlineData(null, FulfillmentTypes.Unknown)]
		[InlineData("", FulfillmentTypes.Unknown)]
		[InlineData("NONE", FulfillmentTypes.Unknown)]
		public async Task GivenACustomerOrderWithVariousDifferentOrderTypeStrings_WhenWeGetModel_ThenAllEnumVariantsShouldBeMappedCorrectly(
			string? databasePropertyValue,
			FulfillmentTypes expectedEnumVariant
		)
		{
			// Arrange
			var orderId = Guid.NewGuid();
			await CreateOneCustomerOrderAsync(orderId, databasePropertyValue);
			// Act
			var customerOrderOption = await customerOrderRepo.GetOneAsync(new CustomerOrder { OrderId = orderId });
			// Assert
			var customerOrderExists = Assert.IsType<AtomicOption<CustomerOrder>.Exists>(customerOrderOption);
			var customerOrder = customerOrderExists.Value;
			Assert.Equal(expectedEnumVariant, customerOrder.FulfillmentType);
		}

		[Fact]
		public async Task WhenWeAttemptToGetOneUsingNull_AnArgumentNullExceptionIsThrown()
		{
			// Assert
			await Assert.ThrowsAsync<ArgumentNullException>(async () =>
			{
				await customerAddressRepo.GetOneAsync(null);
			});
		}

		[Fact]
		public async Task GivenModelHasPropertiesWithAtomsIgnoreAttribute_WhenWeGetIt_ThenIgnoredPropertiesAreSetCorrectly()
		{
			// Arrange
			await CreateOneModelWithIgnoredAsync(1L, "value from database", "value from database");
			// Act
			var modelWithIgnoredOption = await modelWithIgnoredRepo.GetOneAsync(new ModelWithIgnored { Id = 1L });
			// Assert
			var modelWithIgnoredExists = Assert.IsType<AtomicOption<ModelWithIgnored>.Exists>(modelWithIgnoredOption);
			var modelWithIgnored = modelWithIgnoredExists.Value;
			Assert.Equal("value from database", modelWithIgnored.PropertyReadFromButNotWrittenTo);
			Assert.Equal("default", modelWithIgnored.PropertyNeitherReadFromNorWrittenTo);
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
		protected abstract Task CreateOneBlogPostAsync(long postId, BlogPostGenre genre, string title, string content, List<BlogComment>? blogComments = null, bool insertInvalidBlogCommentsJson = false);
		protected abstract Task CreateOneCustomerAddressAsync(string phoneNumber, string city, string country);
		protected abstract Task CreateOneCustomerOrderAsync(Guid orderId, string? orderType);
		protected abstract Task CreateOneModelWithIgnoredAsync(long id, string propertyReadFromButNotWrittenTo, string propertyNeitherReadFromNorWrittenTo);
	}
}
