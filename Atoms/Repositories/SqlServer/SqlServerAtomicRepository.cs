using Atoms.DataAttributes;
using Atoms.Exceptions;
using Atoms.Utils;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Atoms.Utils.Reflection.PropertyInfoRetrieverHelpers;

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

		public async Task<AtomicOption<TModel>> GetOneAsync(TModel modelWithUniqueIdSet)
		{
			using SqlConnection connection = new SqlConnection(connectionString);
			connection.Open();
			var (selectQuery, sqlParameters) =
				SqlTextGenerationUtilities<TModel>
				.GetSelectSqlTextAndParameters(modelWithUniqueIdSet);
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
	}
}
