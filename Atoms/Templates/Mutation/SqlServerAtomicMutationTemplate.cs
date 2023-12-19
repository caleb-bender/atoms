using CalebBender.Atoms.Templates.Parameters;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static CalebBender.Atoms.Utils.Reflection.PropertyInfoRetrieverHelpers;
using static CalebBender.Atoms.Utils.Reflection.TypeMapping.EnumMappingHelpers;

namespace CalebBender.Atoms.Templates.Mutation
{
	internal class SqlServerAtomicMutationTemplate : IAtomicMutationTemplate
	{
		public CancellationToken CancellationToken { get; internal set; }
		internal string ConnectionString { get; init; }
		internal string SqlText { get; init; }
		internal Func<Exception, Task>? ExceptionHandler { get; init; }

		public async Task<int> MutateAsync(object? parameters = null)
		{
			using SqlConnection connection = new SqlConnection(ConnectionString);
			connection.Open();
			using SqlTransaction transaction = connection.BeginTransaction();
			try
			{
				using SqlCommand mutationCommand = connection.CreateCommand();
				mutationCommand.Transaction = transaction;
				var parameterizer = new SqlServerTemplateParameterizer(SqlText, parameters);
				var expandedSqlText = parameterizer.AddParametersAndGetExpandedSqlText(mutationCommand);
				mutationCommand.CommandText = expandedSqlText;
				var numberOfEntriesModified = await mutationCommand.ExecuteNonQueryAsync(CancellationToken);
				await transaction.CommitAsync();
				return numberOfEntriesModified;
			}
			catch (SqlException sqlException)
			{
				await transaction.RollbackAsync();
				if (ExceptionHandler is null) throw;
				await ExceptionHandler.Invoke(sqlException);
				return 0;
			}
		}
	}
}
