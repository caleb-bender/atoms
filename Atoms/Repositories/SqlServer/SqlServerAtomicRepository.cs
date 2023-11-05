using Atoms.DataAttributes;
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
		private static readonly IEnumerable<PropertyInfo> modelPublicProperties = GetAllPublicProperties(typeof(TModel));

        internal SqlServerAtomicRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

		public async Task<AtomicOption<TModel>> GetOneAsync(TModel modelWithUniqueIdSet)
		{
			using SqlConnection connection = new SqlConnection(connectionString);
			connection.Open();
			var (selectQuery, sqlParameters) =
				PropertyMappingUtilities<TModel>
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
			TModel model = GetModelWithMappedProperties(reader);
			return new AtomicOption<TModel>.Exists(model);
		}

		private static TModel GetModelWithMappedProperties(SqlDataReader reader)
		{
			var model = new TModel();
			foreach (var modelProperty in modelPublicProperties)
			{
				var dbPropertyName =
					PropertyMappingUtilities<TModel>.GetDbPropertyNameOfModelProperty(modelProperty);
				var columnValue = reader[dbPropertyName];
				modelProperty.SetValue(model, columnValue);
			}
			return model;
		}
	}
}
