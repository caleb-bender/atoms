﻿using CalebBender.Atoms.Repositories.Factories;
using AtomsIntegrationTests.Models;
using AtomsIntegrationTests.RepositoriesTests.AtomicRepositoryTests;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AtomsIntegrationTests.DatabaseConfig.SqlServer.SqlServerConnection;
using CalebBender.Atoms.Repositories;

namespace AtomsIntegrationTests.RepositoriesTests.SqlServerRepositoryTests
{
	[Collection("SqlServerDBTests")]
	public class SqlServerDeleteManyAsyncTests : DeleteManyAsyncTests
	{

		public SqlServerDeleteManyAsyncTests()
		: base (
			new SqlServerAtomicRepositoryFactory<CustomerAddress>(),
			new SqlServerAtomicRepositoryFactory<BlogUser>(),
			new SqlServerAtomicRepositoryFactory<BlogPost>(),
			new SqlServerAtomicRepositoryFactory<NonexistentModel>(),
			new SqlServerAtomicRepositoryFactory<JobPostingModelEntityMismatch>(),
			new SqlServerAtomicRepositoryFactory<TypeMismatchModel3>(),
            new SqlServerAtomicRepositoryFactory<TimeData>(),
            GetConnectionString()
		)
		{

		}

		protected override void Cleanup()
		{
			using SqlConnection connection = new SqlConnection(GetConnectionString());
			connection.Open();
			using SqlCommand deleteCommand = new SqlCommand(
				@"DELETE FROM CustomerAddresses; DELETE FROM TheBlogUsers;
				DELETE FROM BlogPosts; DELETE FROM JobPostings; DELETE FROM TimeDatas;", connection
			);
			deleteCommand.ExecuteNonQuery();
		}

        protected override IAtomicRepository<T> CreateRepository<T>(string? tableName = null)
        {
            if (tableName is null)
                return new SqlServerAtomicRepositoryFactory<T>().CreateRepository(GetConnectionString());
            else
                return new SqlServerAtomicRepositoryFactory<T>().CreateRepository(GetConnectionString(), tableName);
        }
    }
}
