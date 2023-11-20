using CalebBender.Atoms.Exceptions;
using CalebBender.Atoms.Templates.Query;
using CalebBender.Atoms.Templates.Mutation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalebBender.Atoms.Templates.Builders
{
	/// <summary>
	/// Build an IAtomicQueryTemplate by providing a custom connection string,
	/// custom query text, and if desired, a custom exception handler that will be
	/// called if an exception is thrown.
	/// </summary>
	public class SqlServerRawTemplateBuilder : IRawTemplateBuilder
	{
		private string connectionString = "";
		private string sqlText = "";
		private Func<Exception, Task>? exceptionHandler;
		private CancellationToken cancellationToken = default;

		public IRawTemplateBuilder SetConnectionString(string connectionString)
		{
			this.connectionString = connectionString;
			return this;
		}

		public IAtomicQueryTemplate<T> GetQueryTemplate<T>()
		{
			AssertConnectionStringAndQueryTextWereDefined();
			return new SqlServerAtomicQueryTemplate<T>
			{
				ConnectionString = connectionString,
				SqlText = sqlText,
				ExceptionHandler = exceptionHandler,
				CancellationToken = cancellationToken
			};
		}

		private void AssertConnectionStringAndQueryTextWereDefined()
		{
			if (string.IsNullOrEmpty(connectionString))
				throw new ConnectionStringMissingException("This builder's SetConnectionString adapter method was not called with a nonempty connection string.");
			if (string.IsNullOrEmpty(sqlText))
				throw new QueryTextMissingException("This builder's SetQueryText adapter method was not called with a nonempty query text string.");
		}

		public IAtomicMutationTemplate GetMutationTemplate()
		{
			return new SqlServerAtomicMutationTemplate
			{
				ConnectionString = connectionString,
				SqlText = sqlText,
				ExceptionHandler = exceptionHandler,
				CancellationToken = cancellationToken
			};
		}

		public IRawTemplateBuilder SetQueryText(string queryText)
		{
			sqlText = queryText;
			return this;
		}

		public IRawTemplateBuilder SetExceptionHandler(Func<Exception, Task> exceptionHandler)
		{
			this.exceptionHandler = exceptionHandler;
			return this;
		}

		public IRawTemplateBuilder SetCancellationToken(CancellationToken cancellationToken)
		{
			this.cancellationToken = cancellationToken;
			return this;
		}
	}
}
