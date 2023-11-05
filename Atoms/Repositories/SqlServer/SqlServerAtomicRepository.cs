using Atoms.DataAttributes;
using Atoms.Utils;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Atoms.Utils.Reflection.AttributeCheckerHelpers;
using static Atoms.Utils.Reflection.PropertyInfoRetrieverHelpers;

namespace Atoms.Repositories.SqlServer
{
    internal class SqlServerAtomicRepository<TModel> : IAtomicRepository<TModel>
		where TModel : class, new()
	{
        private string connectionString;
		private static readonly IEnumerable<PropertyInfo> modelPublicProperties = GetAllPublicProperties(typeof(TModel));
		private static readonly IEnumerable<PropertyInfo> modelPublicPropertiesWithUniqueIdAttribute =
			GetPublicPropertiesThatContainAttribute<UniqueIdAttribute>(typeof(TModel));

        internal SqlServerAtomicRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

		public async Task<AtomicOption<TModel>> GetOneAsync(TModel modelWithUniqueIdSet)
		{
			using SqlConnection connection = new SqlConnection(connectionString);
			connection.Open();
			var (selectQuery, sqlParameters) = GetSelectSqlTextAndParameters(modelWithUniqueIdSet);
			return await RetrieveModelAsync(selectQuery, sqlParameters, connection);
		}

		private async Task<AtomicOption<TModel>> RetrieveModelAsync(string selectQuery, IEnumerable<SqlParameter> sqlParameters, SqlConnection connection)
		{
			using SqlCommand readCommand = new SqlCommand(selectQuery, connection);
			readCommand.Parameters.AddRange(sqlParameters.ToArray());
			using SqlDataReader reader = await readCommand.ExecuteReaderAsync();
			await reader.ReadAsync();
			if (reader.HasRows)
			{
				var model = new TModel();
				foreach (var property in modelPublicProperties)
				{
					var propertyName = property.Name;
					try
					{
						var columnValue = reader[propertyName];
						property.SetValue(model, columnValue);
					}
					catch { }
				}
				return new AtomicOption<TModel>.Exists(model);
			}
			return new AtomicOption<TModel>.Empty();
		}
		private static (string, IEnumerable<SqlParameter>) GetSelectSqlTextAndParameters(TModel model)
		{
			var tableName = typeof(TModel).Name + "s";
			var selectQuery = $"SELECT TOP(1) * FROM {tableName}";
			var (whereClause, sqlParameters) = GetWhereClauseTextAndParametersForSpecificModel(model);
			selectQuery += whereClause;
			return (selectQuery, sqlParameters);
		}

		private static (string, IEnumerable<SqlParameter>) GetWhereClauseTextAndParametersForSpecificModel(TModel model)
		{
			List<SqlParameter> sqlParameters = new List<SqlParameter>();
			var whereClause = " WHERE ";
			for (int i = 0; i < modelPublicPropertiesWithUniqueIdAttribute.Count(); i++)
			{
				var property = modelPublicPropertiesWithUniqueIdAttribute.ElementAt(i);
				whereClause += $"{property.Name} = @{property.Name}";
				sqlParameters.Add(new SqlParameter(property.Name, property.GetValue(model)));
				if (i != modelPublicPropertiesWithUniqueIdAttribute.Count() - 1)
					whereClause += " AND ";
			}
			return (whereClause, sqlParameters);
		}
	}
}
