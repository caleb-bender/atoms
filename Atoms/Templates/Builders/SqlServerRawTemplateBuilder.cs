using Atoms.Exceptions;
using Atoms.Templates.Query;
using Atoms.Templates.Mutation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Templates.Builders
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

		public SqlServerRawTemplateBuilder SetConnectionString(string connectionString)
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
				ExceptionHandler = exceptionHandler
			};
		}

		private void AssertConnectionStringAndQueryTextWereDefined()
		{
			if (string.IsNullOrEmpty(connectionString))
				throw new ConnectionStringMissingException("This builder's SetConnectionString adapter method was not called with a nonempty connection string.");
			if (string.IsNullOrEmpty(sqlText))
				throw new QueryTextMissingException("This builder's SetQueryText adapter method was not called with a nonempty query text string.");
		}

		internal IAtomicMutationTemplate GetMutationTemplate()
		{
			return new SqlServerAtomicMutationTemplate
			{
				ConnectionString = connectionString,
				SqlText = sqlText,
				ExceptionHandler = exceptionHandler
			};
		}

		public SqlServerRawTemplateBuilder SetQueryText(string queryText)
		{
			this.sqlText = queryText;
			return this;
		}

		public SqlServerRawTemplateBuilder SetExceptionHandler(Func<Exception, Task> exceptionHandler)
		{
			this.exceptionHandler = exceptionHandler;
			return this;
		}
	}
}
