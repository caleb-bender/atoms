using CalebBender.Atoms.Repositories.Factories;
using AtomsIntegrationTests.Models;
using AtomsIntegrationTests.RepositoriesTests.AtomicRepositoryTests;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static AtomsIntegrationTests.DatabaseConfig.SqlServer.SqlServerConnection;
using static AtomsIntegrationTests.Models.BlogUser;

namespace AtomsIntegrationTests.RepositoriesTests.SqlServerRepositoryTests
{
	[Collection("SqlServerDBTests")]
	public class SqlServerGetOneAsyncTests : GetOneAsyncTests
	{
		public SqlServerGetOneAsyncTests()
		: base(
				new SqlServerAtomicRepositoryFactory<BlogPostAuthor>(),
				new SqlServerAtomicRepositoryFactory<TypeMismatchModel>(),
				new SqlServerAtomicRepositoryFactory<BlogUser>(),
				new SqlServerAtomicRepositoryFactory<BlogPost>(),
				new SqlServerAtomicRepositoryFactory<CustomerAddress>(),
				new SqlServerAtomicRepositoryFactory<CustomerOrder>(),
				new SqlServerAtomicRepositoryFactory<ModelWithIgnored>(),
				new SqlServerAtomicRepositoryFactory<NonexistentModel>(),
				new SqlServerAtomicRepositoryFactory<JobPostingModelEntityMismatch>(),
				new SqlServerAtomicRepositoryFactory<TypeMismatchModel3>(),
				GetConnectionString()
		) { }

		protected override void Cleanup()
		{
			using SqlConnection connection = new SqlConnection(GetConnectionString());
			connection.Open();
			using SqlCommand deleteCommand = new SqlCommand(
				@"DELETE FROM BlogPostAuthors; DELETE FROM TypeMismatchModels;
				DELETE FROM TheBlogUsers; DELETE FROM BlogPosts;
				DELETE FROM CustomerAddresses; DELETE FROM CustomerOrders;
				DELETE FROM ModelsWithIgnored; DELETE FROM JobPostings;",
				connection
			);
			deleteCommand.ExecuteNonQuery();
		}

		protected override async Task CreateJobPostingAsync(long postingId, long employerId)
		{
			using SqlConnection connection = new SqlConnection(GetConnectionString());
			connection.Open();
			using SqlCommand createCommand = new SqlCommand(
				$@"INSERT INTO JobPostings(PostingId, EmployerId)
				VALUES ({postingId}, '{employerId}')", connection
			);
			await createCommand.ExecuteNonQueryAsync();
		}

		protected override async Task CreateOneBlogPostAsync(long postId, BlogPost.BlogPostGenre genre, string title, string content, List<BlogComment>? blogComments = null, bool insertInvalidBlogCommentsJson = false)
		{
			using SqlConnection connection = new SqlConnection(GetConnectionString());
			connection.Open();
			var insertIntoText = "INSERT INTO BlogPosts(PostId, Genre, Title, Content";
			var valuesText = $" VALUES ({postId}, '{genre}', '{title}', '{content}'";
			if (blogComments is not null || insertInvalidBlogCommentsJson is true)
			{
				insertIntoText += ", BlogComments";
				var blogCommentsSerialized = JsonConvert.SerializeObject(blogComments);
				valuesText += $", '{(insertInvalidBlogCommentsJson ? "}invalid{}{" : blogCommentsSerialized)}'";
			}
			insertIntoText += ")";
			valuesText += ")";
			using SqlCommand createCommand = new SqlCommand(
				insertIntoText + valuesText, connection
			);
			await createCommand.ExecuteNonQueryAsync();
		}

		protected override async Task CreateOneBlogPostAuthorAsync(long authorId, string authorName, DateTime authorSinceDate)
		{
			using SqlConnection connection = new SqlConnection(GetConnectionString());
			connection.Open();
			using SqlCommand createCommand = new SqlCommand(
				$@"INSERT INTO BlogPostAuthors(AuthorId, AuthorName, UserRegisteredOn)
				VALUES ({authorId}, '{authorName}', '{authorSinceDate}')", connection
			);
			await createCommand.ExecuteNonQueryAsync();
		}

		protected override async Task CreateOneBlogUserAsync(long userId, string groupName, string? userRole = null)
		{
			if (userRole is null) userRole = BlogUserRole.Reader.ToString();
			using SqlConnection connection = new SqlConnection(GetConnectionString());
			connection.Open();
			using SqlCommand createCommand = new SqlCommand(
				$@"INSERT INTO TheBlogUsers(UserId, GroupId, UserRole)
				VALUES ({userId}, '{groupName}', '{userRole}')", connection
			);
			await createCommand.ExecuteNonQueryAsync();
		}

		protected override async Task CreateOneCustomerAddressAsync(string phoneNumber, string city, string country)
		{
			using SqlConnection connection = new SqlConnection(GetConnectionString());
			connection.Open();
			using SqlCommand createCommand = new SqlCommand(
				$@"INSERT INTO CustomerAddresses(Phone, City, Country)
				VALUES ('{phoneNumber}', '{city}', '{country}')", connection
			);
			await createCommand.ExecuteNonQueryAsync();
		}

		protected override async Task CreateOneCustomerOrderAsync(Guid orderId, string? orderType)
		{
			using SqlConnection connection = new SqlConnection(GetConnectionString());
			connection.Open();
			var insertIntoText = "INSERT INTO CustomerOrders(OrderId";
			var valuesText = $" VALUES('{orderId}'";
			if (orderType is not null)
			{
				insertIntoText += ", OrderType";
				valuesText += $", '{orderType}'";
			}
			insertIntoText += ")";
			valuesText += ")";
			using SqlCommand createCommand = new SqlCommand(
				insertIntoText + valuesText, connection
			);
			await createCommand.ExecuteNonQueryAsync();
		}

		protected override async Task CreateOneModelWithIgnoredAsync(long id, string propertyReadFromButNotWrittenTo, string propertyNeitherReadFromNorWrittenTo)
		{
			using SqlConnection connection = new SqlConnection(GetConnectionString());
			connection.Open();
			using SqlCommand createCommand = new SqlCommand(
				$@"INSERT INTO ModelsWithIgnored(Id, PropertyReadFromButNotWrittenTo, PropertyNeitherReadFromNorWrittenTo)
				VALUES ('{id}', '{propertyReadFromButNotWrittenTo}', '{propertyNeitherReadFromNorWrittenTo}')", connection
			);
			await createCommand.ExecuteNonQueryAsync();
		}

		protected override async Task CreateOneTypeMismatchModelAsync(Guid id, string status)
		{
			using SqlConnection connection = new SqlConnection(GetConnectionString());
			connection.Open();
			using SqlCommand createCommand = new SqlCommand(
				$@"INSERT INTO TypeMismatchModels(Id, Status)
				VALUES ('{id}', '{status}')", connection
			);
			await createCommand.ExecuteNonQueryAsync();
		}
	}
}
