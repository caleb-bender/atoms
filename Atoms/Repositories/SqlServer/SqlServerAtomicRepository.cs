using Atoms.Exceptions;
using Atoms.Repositories.SqlServer.SqlGeneration;
using Atoms.Utils;
using System.Data.SqlClient;
using System.Reflection;
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

		public async Task<IEnumerable<TModel>> CreateManyAsync(IEnumerable<TModel> models)
		{
			using SqlConnection connection = new SqlConnection(connectionString);
			connection.Open();
			using SqlTransaction transaction = connection.BeginTransaction();
			try
			{
				await InsertModelsAsync(transaction, models);
				await transaction.CommitAsync();
				return models;
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
				SelectSqlGenerator<TModel>
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

		private async Task InsertModelsAsync(SqlTransaction transaction, IEnumerable<TModel> models)
		{
			var (insertSqlText, insertParameters, identityModelProperty) =
				InsertSqlGenerator<TModel>.GetInsertSqlText(models);
			using SqlCommand insertCommand = new SqlCommand(insertSqlText, transaction.Connection);
			insertCommand.Parameters.AddRange(insertParameters.ToArray());
			insertCommand.Transaction = transaction;
			if (identityModelProperty is null)
				await insertCommand.ExecuteNonQueryAsync();
			else
			{
				object identityValue = await insertCommand.ExecuteScalarAsync() ?? 0;
				identityValue =
					InsertSqlGenerator<TModel>.GetConvertedIdentityType(identityModelProperty, identityValue);
				identityModelProperty.SetValue(models.First(), identityValue);
			}
		}
	}
}
