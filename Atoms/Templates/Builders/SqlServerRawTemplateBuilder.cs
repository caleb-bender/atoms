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
	public class SqlServerRawTemplateBuilder
	{
		private string connectionString = "";
		private string sqlText = "";
		private Func<Exception, Task>? exceptionHandler;

		public SqlServerRawTemplateBuilder SetConnectionString(string connectionString)
		{
			this.connectionString = connectionString;
			return this;
		}

		public IAtomicQueryTemplate<T> GetQueryTemplate<T>() {
			return new SqlServerAtomicQueryTemplate<T>
			{
				ConnectionString = connectionString,
				SqlText = sqlText,
				ExceptionHandler = exceptionHandler
			};
		}
		public IAtomicMutationTemplate GetMutationTemplate() {
			return new SqlServerAtomicMutationTemplate
			{
				ConnectionString = connectionString,
				SqlText = sqlText,
				ExceptionHandler = exceptionHandler
			};
		}

		public SqlServerRawTemplateBuilder SetSqlText(string sqlText)
		{
			this.sqlText = sqlText;
			return this;
		}

		public SqlServerRawTemplateBuilder SetExceptionHandler(Func<Exception, Task> exceptionHandler)
		{
			this.exceptionHandler = exceptionHandler;
			return this;
		}
	}
}
