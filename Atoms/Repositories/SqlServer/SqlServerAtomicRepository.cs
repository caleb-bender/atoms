using CalebBender.Atoms.Exceptions;
using CalebBender.Atoms.Repositories.SqlServer.SqlGeneration;
using CalebBender.Atoms.Utils;
using System.Data.SqlClient;
using System.Reflection;
using static CalebBender.Atoms.Repositories.SqlServer.SqlServerErrorTranslators;

namespace CalebBender.Atoms.Repositories.SqlServer
{
    internal class SqlServerAtomicRepository<TModel> : IAtomicRepository<TModel>
		where TModel : class, new()
	{
        private string connectionString;

        internal SqlServerAtomicRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

		public async Task<int> UpdateManyAsync(IEnumerable<TModel> models)
		{
			if (models is null || models.Count() == 0) return 0;
			using SqlConnection connection = new SqlConnection(connectionString);
			connection.Open();
			using SqlTransaction transaction = connection.BeginTransaction();
			try
			{
				var numberUpdated = await UpdateModelsAsync(transaction, models);
				await transaction.CommitAsync();
				return numberUpdated;
			}
			catch (SqlException sqlException)
			{
				await transaction.RollbackAsync();
				throw;
			}
		}

		private async Task<int> UpdateModelsAsync(SqlTransaction transaction, IEnumerable<TModel> models)
		{
			var (updateSqlText, updateParameters) =
				UpdateSqlGenerator<TModel>.GetUpdateSqlTextAndParameters(models);
			using SqlCommand updateCommand = new SqlCommand(updateSqlText, transaction.Connection);
			updateCommand.Parameters.AddRange(updateParameters.ToArray());
			updateCommand.Transaction = transaction;
			return await updateCommand.ExecuteNonQueryAsync();
		}

		public async Task<IEnumerable<TModel>> CreateManyAsync(IEnumerable<TModel> models)
		{
			if (models is null || models.Count() == 0) return new List<TModel>();
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
				TranslateOperandTypeClashError(sqlException, typeof(TModel));
				TranslateInvalidColumnNameError(sqlException, typeof(TModel));
				TranslateInvalidObjectNameError(sqlException, typeof(TModel));
				throw;
			}
		}

		public async Task<AtomicOption<TModel>> GetOneAsync(TModel model)
		{
			if (model is null) throw new ArgumentNullException("A null model cannot be used in GetOneAsync.");
			using SqlConnection connection = new SqlConnection(connectionString);
			connection.Open();
			try
			{
				var (selectQuery, sqlParameters) =
					SelectSqlGenerator<TModel>
					.GetSelectSqlTextAndParameters(model);
				return await RetrieveModelAsync(selectQuery, sqlParameters, connection);
			}
			catch (SqlException err)
			{
				TranslateOperandTypeClashError(err, typeof(TModel));
				TranslateInvalidColumnNameError(err, typeof(TModel));
				TranslateInvalidObjectNameError(err, typeof(TModel));
				throw;
			}
		}

		public async Task<int> DeleteManyAsync(IEnumerable<TModel> models)
		{
			if (models is null || models.Count() == 0) return 0;
			using SqlConnection connection = new SqlConnection(connectionString);
			connection.Open();
			using SqlTransaction transaction = connection.BeginTransaction();
			try
			{
				var numberOfRowsDeleted = await DeleteModelsAsync(transaction, models);
				await transaction.CommitAsync();
				return numberOfRowsDeleted;
			}
			catch (SqlException sqlException)
			{
				await transaction.RollbackAsync();
				TranslateOperandTypeClashError(sqlException, typeof(TModel));
				TranslateInvalidColumnNameError(sqlException, typeof(TModel));
				TranslateInvalidObjectNameError(sqlException, typeof(TModel));
				throw;
			}
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
			var (insertSqlText, insertParameters) =
				InsertSqlGenerator<TModel>.GetInsertSqlTextAndParameters(models);
			using SqlCommand insertCommand = new SqlCommand(insertSqlText, transaction.Connection);
			insertCommand.Parameters.AddRange(insertParameters.ToArray());
			insertCommand.Transaction = transaction;
			using var identitiesReader = await insertCommand.ExecuteReaderAsync();
			int i = 0;
			while (identitiesReader.Read())
			{
				var model = models.ElementAt(i);
				var identityValue = identitiesReader["Id"] ?? 0;
				identityValue = InsertSqlGenerator<TModel>.GetConvertedIdentityType(identityValue);
				var identityProperty = ModelMetadata<TModel>.FirstAutoGeneratedUniqueIdProperty;
				identityProperty?.SetValue(model, identityValue);
				i++;
			}
		}

		private async Task<int> DeleteModelsAsync(SqlTransaction transaction, IEnumerable<TModel> models)
		{
			var (deleteSqlText, deleteParameters) =
				DeleteSqlGenerator<TModel>.GetDeleteTextAndParameters(models);
			using SqlCommand deleteCommand = new SqlCommand(deleteSqlText, transaction.Connection);
			deleteCommand.Parameters.AddRange(deleteParameters.ToArray());
			deleteCommand.Transaction = transaction;
			return await deleteCommand.ExecuteNonQueryAsync();
		}
	}
}
