﻿using Atoms.Exceptions;
using Atoms.Templates.Builders;
using Atoms.Templates.Mutation;
using Atoms.Templates.Query;

namespace AtomsUnitTests.TemplatesTests.BuildersTests.SqlServer
{
    public class SqlServerTemplateBuilderTests
	{
		private readonly SqlServerRawTemplateBuilder templateBuilder;
		private bool exceptionHandler1WasCalled = false;
		private bool exceptionHandler2WasCalled = false;

		public SqlServerTemplateBuilderTests()
		{
			templateBuilder = new SqlServerRawTemplateBuilder();
		}

		[Fact]
		public void WhenConnectionStringIsSet_And_TemplateIsRetrieved_ThenTheTemplateHasTheConnectionString()
		{
			// Act
			var queryTemplate = new SqlServerRawTemplateBuilder()
				.SetConnectionString("connection")
				.GetQueryTemplate<int>();

			var mutationTemplate = new SqlServerRawTemplateBuilder()
				.SetConnectionString("connection")
				.GetMutationTemplate();

			// Assert
			var sqlServerQueryTemplate = Assert.IsType<SqlServerQueryTemplate<int>>(queryTemplate);
			var sqlServerMutationTemplate = Assert.IsType<SqlServerMutationTemplate>(mutationTemplate);

			Assert.Equal("connection", sqlServerQueryTemplate.ConnectionString);
			Assert.Equal("connection", sqlServerMutationTemplate.ConnectionString);
		}

		[Fact]
		public void WhenSqlTextIsSet_And_WhenWeGetTemplate_ItHasTheSqlText()
		{
			// Act
			var queryTemplate = new SqlServerRawTemplateBuilder()
				.SetSqlText("SQL")
				.GetQueryTemplate<int>();
			var mutationTemplate = new SqlServerRawTemplateBuilder()
				.SetSqlText("SQL")
				.GetMutationTemplate();
			// Assert
			var sqlServerQueryTemplate = Assert.IsType<SqlServerQueryTemplate<int>>(queryTemplate);
			var sqlServerMutationTemplate = Assert.IsType<SqlServerMutationTemplate>(mutationTemplate);

			Assert.Equal("SQL", sqlServerQueryTemplate.SqlText);
			Assert.Equal("SQL", sqlServerMutationTemplate.SqlText);
		}

		private Task ExceptionHandler1(Exception exception)
		{
			exceptionHandler1WasCalled = true;
			return Task.CompletedTask;
		}

		private Task ExceptionHandler2(Exception exception)
		{
			exceptionHandler2WasCalled = true;
			return Task.CompletedTask;
		}

		[Fact]
		public void WhenExceptionHandlerIsSet_And_WhenWeGetTemplate_ThenItContainsOurExceptionCallback()
		{
			// Act
			var queryTemplate = new SqlServerRawTemplateBuilder()
				.SetExceptionHandler(ExceptionHandler1)
				.GetQueryTemplate<int>();
			var mutationTemplate = new SqlServerRawTemplateBuilder()
				.SetExceptionHandler(ExceptionHandler2)
				.GetMutationTemplate();
			// Assert
			var sqlServerQueryTemplate = Assert.IsType<SqlServerQueryTemplate<int>>(queryTemplate);
			var sqlServerMutationTemplate = Assert.IsType<SqlServerMutationTemplate>(mutationTemplate);

			sqlServerQueryTemplate.ExceptionHandler?.Invoke(new Exception());
			sqlServerMutationTemplate.ExceptionHandler?.Invoke(new Exception());

			Assert.True(exceptionHandler1WasCalled);
			Assert.True(exceptionHandler2WasCalled);
		}
	}
}
