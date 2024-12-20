﻿using AtomsIntegrationTests.Models;
using AtomsIntegrationTests.RepositoriesTests.AtomicRepositoryTests;
using CalebBender.Atoms.Repositories;
using CalebBender.Atoms.Repositories.Factories;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AtomsIntegrationTests.DatabaseConfig.SqlServer.SqlServerConnection;
using static AtomsIntegrationTests.Models.CustomerOrder;

namespace AtomsIntegrationTests.RepositoriesTests.SqlServerRepositoryTests
{
	[Collection("SqlServerDBTests")]
	public class SqlServerUpdateManyAsyncTests : UpdateManyAsyncTests
	{
		protected override void Cleanup()
		{
			using SqlConnection connection = new SqlConnection(GetConnectionString());
			connection.Open();
			using SqlCommand deleteCommand = new SqlCommand(
                @"DELETE FROM Employees; DELETE FROM CustomerAddresses; DELETE FROM BlogPosts;
					DELETE FROM CustomerOrders; DELETE FROM ModelsWithIgnored;
					DELETE FROM JobPostings; DELETE FROM HolidayMatrices; DELETE FROM TimeDatas;",
				connection);
			deleteCommand.ExecuteNonQuery();
		}

		protected override IAtomicRepository<T> CreateRepository<T>(string? tableName = null)
		{
			if (tableName is null)
				return new SqlServerAtomicRepositoryFactory<T>().CreateRepository(GetConnectionString());
			else
				return new SqlServerAtomicRepositoryFactory<T>().CreateRepository(GetConnectionString(), tableName);
		}

		[Theory]
		[InlineData("PC", FulfillmentTypes.PickupByCustomer)]
		[InlineData("D", FulfillmentTypes.Delivery)]
		[InlineData("PT", FulfillmentTypes.PickupByThirdParty)]
		[InlineData("NONE", FulfillmentTypes.Unknown)]
		public async Task GivenAModelContainsStringToEnumMappingRulesForAnEnumProperty_WhenWeUpdateIt_ThenThoseMappingsAreSavedToTheDatabase(
			string? expectedDatabasePropertyValue,
			FulfillmentTypes enumVariant
		)
		{
			// Arrange
			var customerOrderRepo = CreateRepository<CustomerOrder>();
			var orderId = Guid.NewGuid();
			var customerOrder = new CustomerOrder { OrderId = orderId, FulfillmentType = FulfillmentTypes.Dropoff };
			await customerOrderRepo.CreateOneAsync(customerOrder);
			// Act
			customerOrder.FulfillmentType = enumVariant;
			await customerOrderRepo.UpdateOneAsync(customerOrder);
			// Assert
			using SqlConnection connection = new SqlConnection(GetConnectionString());
			string actualDatabasePropertyValue = (await connection.QueryAsync<string>($"SELECT OrderType FROM CustomerOrders WHERE OrderId = '{orderId}'")).First();
			Assert.Equal(expectedDatabasePropertyValue, actualDatabasePropertyValue);
		}
	}
}
