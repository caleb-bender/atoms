using Atoms.Repositories;
using Atoms.Repositories.Factories;
using Atoms.Templates.Builders;
using Atoms.Templates.Query;
using AtomsIntegrationTests.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AtomsIntegrationTests.DatabaseConfig.SqlServer.SqlServerConnection;

namespace AtomsIntegrationTests.TemplateTests.QueryTemplateTests
{
	[Collection("SqlServerDBTests")]
	public class SqlServerAtomicQueryTemplateTests : AtomicQueryTemplateTests
	{
		private static readonly string connectionString = GetConnectionString();
		private static readonly IAtomicQueryTemplate<long> blogPostIdQueryTemplate = GetBlogPostIdQueryTemplate();
		private static readonly IAtomicQueryTemplate<(long, BlogPost.BlogPostGenre, string)> blogIdGenreTitleQueryTemplate =
			GetIdGenreTitleQueryTemplate();
		private static readonly IAtomicQueryTemplate<(Guid, CustomerOrder.FulfillmentTypes)> customerOrderIdAndTypeQueryTemplate =
			GetCustomerOrderIdAndTypeQueryTemplate();
		private static readonly IAtomicQueryTemplate<string?> customerCityQueryTemplate =
			GetCustomerCityQueryTemplate();

		private static IAtomicQueryTemplate<string?> GetCustomerCityQueryTemplate()
		{
			return new SqlServerRawTemplateBuilder()
				.SetConnectionString(connectionString)
				.SetSqlText("SELECT City FROM CustomerAddresses")
				.GetQueryTemplate<string?>();
		}

		private static IAtomicQueryTemplate<(long, BlogPost.BlogPostGenre, string)> GetIdGenreTitleQueryTemplate()
		{
			return new SqlServerRawTemplateBuilder()
				.SetConnectionString(connectionString)
				.SetSqlText("SELECT PostId, Genre, Title FROM BlogPosts")
				.GetQueryTemplate<(long, BlogPost.BlogPostGenre, string)>();
		}

		public SqlServerAtomicQueryTemplateTests()
		: base(
			blogPostIdQueryTemplate, blogIdGenreTitleQueryTemplate,
			customerOrderIdAndTypeQueryTemplate, customerCityQueryTemplate
		)
		{
		}

		private static IAtomicQueryTemplate<long> GetBlogPostIdQueryTemplate()
		{
			return new SqlServerRawTemplateBuilder()
				.SetConnectionString(connectionString)
				.SetSqlText("SELECT PostId FROM BlogPosts")
				.GetQueryTemplate<long>();
		}

		private static IAtomicQueryTemplate<(Guid, CustomerOrder.FulfillmentTypes)> GetCustomerOrderIdAndTypeQueryTemplate()
		{
			return new SqlServerRawTemplateBuilder()
				.SetConnectionString(connectionString)
				.SetSqlText("SELECT OrderId, OrderType FROM CustomerOrders")
				.GetQueryTemplate<(Guid, CustomerOrder.FulfillmentTypes)>();
		}

		protected override void Cleanup()
		{
			using SqlConnection connection = new SqlConnection(connectionString);
			connection.Open();
			using SqlCommand deleteCommand = new SqlCommand(
				@"DELETE FROM BlogPosts; DELETE FROM CustomerOrders;
				DELETE FROM CustomerAddresses;", connection
			);
			deleteCommand.ExecuteNonQuery();
		}

		protected override IAtomicRepository<T> GetAtomicRepository<T>()
		{
			return new SqlServerAtomicRepositoryFactory<T>().CreateRepository(connectionString);
		}
	}
}
