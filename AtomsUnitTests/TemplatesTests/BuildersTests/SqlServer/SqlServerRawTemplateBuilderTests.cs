using CalebBender.Atoms.Exceptions;
using CalebBender.Atoms.Templates.Builders;
using CalebBender.Atoms.Templates.Mutation;
using CalebBender.Atoms.Templates.Query;

namespace AtomsUnitTests.TemplatesTests.BuildersTests.SqlServer
{
    public class SqlServerRawTemplateBuilderTests
	{
		private readonly SqlServerRawTemplateBuilder templateBuilder;
		private bool exceptionHandler1WasCalled = false;
		private bool exceptionHandler2WasCalled = false;

		public SqlServerRawTemplateBuilderTests()
		{
			templateBuilder = new SqlServerRawTemplateBuilder();
		}

		[Fact]
		public void WhenConnectionStringIsSet_And_TemplateIsRetrieved_ThenTheTemplateHasTheConnectionString()
		{
			// Act
			var queryTemplate = new SqlServerRawTemplateBuilder()
				.SetConnectionString("connection")
				.SetQueryText("SQL")
				.GetQueryTemplate<int>();

			var mutationTemplate = new SqlServerRawTemplateBuilder()
				.SetConnectionString("connection")
				.SetMutationText("SQL")
				.GetMutationTemplate();

			// Assert
			var sqlServerQueryTemplate = Assert.IsType<SqlServerAtomicQueryTemplate<int>>(queryTemplate);
			var sqlServerMutationTemplate = Assert.IsType<SqlServerAtomicMutationTemplate>(mutationTemplate);

			Assert.Equal("connection", sqlServerQueryTemplate.ConnectionString);
			Assert.Equal("connection", sqlServerMutationTemplate.ConnectionString);
		}

		[Fact]
		public void WhenSqlTextIsSet_And_WhenWeGetTemplate_ItHasTheSqlText()
		{
			// Act
			var queryTemplate = new SqlServerRawTemplateBuilder()
				.SetConnectionString("connection")
				.SetQueryText("SQL")
				.GetQueryTemplate<int>();
			var mutationTemplate = new SqlServerRawTemplateBuilder()
				.SetConnectionString("connection")
				.SetMutationText("SQL")
				.GetMutationTemplate();
			// Assert
			var sqlServerQueryTemplate = Assert.IsType<SqlServerAtomicQueryTemplate<int>>(queryTemplate);
			var sqlServerMutationTemplate = Assert.IsType<SqlServerAtomicMutationTemplate>(mutationTemplate);

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
				.SetConnectionString("connection")
				.SetQueryText("SQL")
				.SetExceptionHandler(ExceptionHandler1)
				.GetQueryTemplate<int>();
			var mutationTemplate = new SqlServerRawTemplateBuilder()
				.SetConnectionString("connection")
				.SetQueryText("SQL")
				.SetExceptionHandler(ExceptionHandler2)
				.GetMutationTemplate();
			// Assert
			var sqlServerQueryTemplate = Assert.IsType<SqlServerAtomicQueryTemplate<int>>(queryTemplate);
			var sqlServerMutationTemplate = Assert.IsType<SqlServerAtomicMutationTemplate>(mutationTemplate);

			sqlServerQueryTemplate.ExceptionHandler?.Invoke(new Exception());
			sqlServerMutationTemplate.ExceptionHandler?.Invoke(new Exception());

			Assert.True(exceptionHandler1WasCalled);
			Assert.True(exceptionHandler2WasCalled);
		}

		[Fact]
		public void WhenWeGetQueryTemplateWithoutDefiningAConnectionString_ThenAConnectionStringMissingExceptionIsThrown()
		{
			// Assert
			Assert.Throws<ConnectionStringMissingException>(() =>
			{
				var queryTemplate = new SqlServerRawTemplateBuilder()
				.SetQueryText("SQL")
				.SetExceptionHandler(ExceptionHandler1)
				.GetQueryTemplate<int>();
			});
		}

		[Fact]
		public void WhenWeGetMutationTemplateWithoutDefiningAConnectionString_ThenAConnectionStringMissingExceptionIsThrown()
		{
			// Assert
			Assert.Throws<ConnectionStringMissingException>(() =>
			{
				var queryTemplate = new SqlServerRawTemplateBuilder()
				.SetMutationText("SQL")
				.SetExceptionHandler(ExceptionHandler1)
				.GetMutationTemplate();
			});
		}

		[Fact]
		public void WhenWeGetQueryTemplateWithoutDefiningTheQueryText_ThenQueryTextMissingExceptionIsThrown()
		{
			Assert.Throws<QueryTextMissingException>(() =>
			{
				var queryTemplate = new SqlServerRawTemplateBuilder()
				.SetConnectionString("connection")
				.SetExceptionHandler(ExceptionHandler1)
				.GetQueryTemplate<int>();
			});
		}

		[Fact]
		public void WhenWeGetMutationTemplateWithoutDefiningTheMutationText_ThenMutationTextMissingExceptionIsThrown()
		{
			Assert.Throws<MutationTextMissingException>(() =>
			{
				var mutationTemplate = new SqlServerRawTemplateBuilder()
				.SetConnectionString("connection")
				.SetExceptionHandler(ExceptionHandler1)
				.GetMutationTemplate();
			});
		}

		[Fact]
		public void WhenWeDefineACustomCancellationToken_And_GetTemplates_ThenTheTemplatesContainTheToken()
		{
			// Arrange
			var cancellationTokenSource = new CancellationTokenSource();
			var cancellationToken = cancellationTokenSource.Token;
			cancellationTokenSource.Cancel();
			// Act
			var queryTemplate = new SqlServerRawTemplateBuilder()
				.SetConnectionString("connection")
				.SetQueryText("SQL")
				.SetCancellationToken(cancellationToken)
				.GetQueryTemplate<int>();
			var mutationTemplate = new SqlServerRawTemplateBuilder()
				.SetConnectionString("connection")
				.SetQueryText("SQL")
				.SetCancellationToken(cancellationToken)
				.GetMutationTemplate();
			// Assert
			var sqlServerQueryTemplate = Assert.IsType<SqlServerAtomicQueryTemplate<int>>(queryTemplate);
			var sqlServerMutationTemplate = Assert.IsType<SqlServerAtomicMutationTemplate>(mutationTemplate);
			Assert.Equal(cancellationToken, sqlServerQueryTemplate.CancellationToken);
			Assert.Equal(cancellationToken, sqlServerMutationTemplate.CancellationToken);
		}
	}
}
