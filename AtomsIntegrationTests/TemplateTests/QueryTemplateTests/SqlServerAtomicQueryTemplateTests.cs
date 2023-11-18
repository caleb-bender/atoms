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
		private static readonly IAtomicRepository<BlogPost> blogPostRepo =
			new SqlServerAtomicRepositoryFactory<BlogPost>().CreateRepository(connectionString);
		private static readonly IAtomicQueryTemplate<long> blogPostIdQueryTemplate = GetBlogPostIdQueryTemplate();
		private static readonly IAtomicQueryTemplate<(long, BlogPost.BlogPostGenre, string)> blogIdGenreTitleQueryTemplate =
			GetIdGenreTitleQueryTemplate();

		private static IAtomicQueryTemplate<(long, BlogPost.BlogPostGenre, string)> GetIdGenreTitleQueryTemplate()
		{
			return new SqlServerRawTemplateBuilder()
				.SetConnectionString(connectionString)
				.SetSqlText("SELECT PostId, Genre, Title FROM BlogPosts")
				.GetQueryTemplate<(long, BlogPost.BlogPostGenre, string)>();
		}

		public SqlServerAtomicQueryTemplateTests()
			: base(blogPostRepo, blogPostIdQueryTemplate, blogIdGenreTitleQueryTemplate)
		{
		}

		private static IAtomicQueryTemplate<long> GetBlogPostIdQueryTemplate()
		{
			return new SqlServerRawTemplateBuilder()
				.SetConnectionString(connectionString)
				.SetSqlText("SELECT PostId FROM BlogPosts")
				.GetQueryTemplate<long>();
		}

		protected override void Cleanup()
		{
			using SqlConnection connection = new SqlConnection(connectionString);
			connection.Open();
			using SqlCommand deleteCommand = new SqlCommand(
				@"DELETE FROM BlogPosts;", connection
			);
			deleteCommand.ExecuteNonQuery();
		}
	}
}
