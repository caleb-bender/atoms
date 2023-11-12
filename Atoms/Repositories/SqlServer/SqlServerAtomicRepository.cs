using Atoms.Exceptions;
using Atoms.Utils;
using System.Data.SqlClient;
using static Atoms.Repositories.SqlServer.SqlServerErrorTranslators;

namespace Atoms.Repositories.SqlServer
{
    internal class SqlServerAtomicRepository<TModel> : IAtomicRepository<TModel>
		where TModel : class, new()
	{
        private string connectionString;

        internal SqlServerAtomicRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

		public async Task<TModel> CreateOneAsync(TModel model)
		{
			using SqlConnection connection = new SqlConnection(connectionString);
			connection.Open();
			using SqlTransaction transaction = connection.BeginTransaction();
			try
			{
				await InsertModelAsync(transaction, model);
				await transaction.CommitAsync();
				return model;
			}
			catch (SqlException sqlException)
			{
				await transaction.RollbackAsync();
				TranslateDuplicatePrimaryKeyOrIndexError(sqlException, typeof(TModel));
				throw;
			}
		}

		public async Task<AtomicOption<TModel>> GetOneAsync(TModel model)
		{
			using SqlConnection connection = new SqlConnection(connectionString);
			connection.Open();
			var (selectQuery, sqlParameters) =
				SqlTextGenerationUtilities<TModel>
				.GetSelectSqlTextAndParameters(model);
			return await RetrieveModelAsync(selectQuery, sqlParameters, connection);
		}

		private async Task<AtomicOption<TModel>> RetrieveModelAsync(string selectQuery, IEnumerable<SqlParameter> sqlParameters, SqlConnection connection)
		{
			using SqlCommand readCommand = new SqlCommand(selectQuery, connection);
			readCommand.Parameters.AddRange(sqlParameters.ToArray());
			using SqlDataReader reader = await readCommand.ExecuteReaderAsync();
			await reader.ReadAsync();
			if (!reader.HasRows) return new AtomicOption<TModel>.Empty();
			TModel model =
				PropertyMappingUtilities<TModel>
				.GetModelWithMappedProperties(reader);
			return new AtomicOption<TModel>.Exists(model);
		}

		private async Task InsertModelAsync(SqlTransaction transaction, TModel model)
		{
			var (insertSqlText, insertParameters) = SqlTextGenerationUtilities<TModel>.GetInsertSqlTextForOne(model);
			using SqlCommand insertCommand = new SqlCommand(insertSqlText, transaction.Connection);
			insertCommand.Parameters.AddRange(insertParameters.ToArray());
			insertCommand.Transaction = transaction;
			await insertCommand.ExecuteNonQueryAsync();
		}
	}
}
