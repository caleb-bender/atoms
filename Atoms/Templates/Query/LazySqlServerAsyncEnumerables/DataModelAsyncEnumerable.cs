using Atoms.Exceptions;
using Atoms.Repositories.SqlServer;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Atoms.Repositories.SqlServer.SqlServerErrorTranslators;

namespace Atoms.Templates.Query.LazySqlServerAsyncEnumerables
{
	internal class DataModelAsyncEnumerable<TModel> : LazyAsyncEnumerable<TModel>
	{
		private static readonly Type modelType = typeof(TModel);

		public DataModelAsyncEnumerable(
			string connectionString, string sqlText, object? parameters,
			Func<Exception, Task>? exceptionHandler,
			CancellationToken cancellationToken
		) : base(connectionString, sqlText, parameters, exceptionHandler, cancellationToken)
		{
		}

		protected override TModel GetValueFromReader(SqlDataReader reader)
		{
			try
			{
				var model = PropertyMappingUtilities<TModel>.GetModelWithMappedProperties(reader);
				return model;

			}
			catch (IndexOutOfRangeException err)
			{
				throw new ModelDbEntityMismatchException(
					$@"The schema of the model ""{modelType.Name}"" does not match its corresponding database entity schema.
					Make sure the name of the model and its public properties match the database entity schema.
					If the model name or any of its properties' names don't match, consider manually defining names by annotating
					with DbEntityNameAttribute or DbPropertyNameAttribute respectively.", err);
			}
		}
	}
}
