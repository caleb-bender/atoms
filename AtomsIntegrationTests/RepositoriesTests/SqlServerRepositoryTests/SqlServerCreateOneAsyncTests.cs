﻿using Atoms.Repositories.Factories;
using AtomsIntegrationTests.Models;
using AtomsIntegrationTests.RepositoriesTests.AtomicRepositoryTests;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AtomsIntegrationTests.DatabaseConfig.SqlServer.SqlServerConnection;

namespace AtomsIntegrationTests.RepositoriesTests.SqlServerRepositoryTests
{
	[Collection("SqlServerDBTests")]
	public class SqlServerCreateOneAsyncTests : CreateOneAsyncTests
	{
		public SqlServerCreateOneAsyncTests()
			: base(
				  new SqlServerAtomicRepositoryFactory<BlogPostAuthor>(),
				  new SqlServerAtomicRepositoryFactory<CustomerAddress>(),
				  new SqlServerAtomicRepositoryFactory<JobPosting>(),
				  GetConnectionString()
			)
		{}

		protected override void Cleanup()
		{
			using SqlConnection connection = new SqlConnection(GetConnectionString());
			connection.Open();
			using SqlCommand deleteCommand = new SqlCommand(
				@"DELETE FROM BlogPostAuthors; DELETE FROM CustomerAddresses;
				DELETE FROM JobPostings;",
				connection
			);
			deleteCommand.ExecuteNonQuery();
		}
	}
}