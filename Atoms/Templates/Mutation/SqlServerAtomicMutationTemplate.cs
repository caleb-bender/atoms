using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
				using SqlCommand mutationCommand = new SqlCommand(SqlText, connection);
				mutationCommand.Transaction = transaction;
				await mutationCommand.ExecuteNonQueryAsync();
				await transaction.CommitAsync();
			}
			catch (SqlException sqlException)
			{
				await transaction.RollbackAsync();
				throw;
			}
			return 1;
		}
	}
}
