using Atoms.DataAttributes;
using Atoms.Exceptions;
using Atoms.Utils;
using System.Data.SqlClient;

namespace Atoms.Repositories.Factories
{
    public class SqlServerAtomicRepositoryFactory<TModel> : AtomicRepositoryFactory<TModel>
		where TModel : class, new()
	{
		protected override IAtomicRepository<TModel> NewAtomicRepositoryResult(string connectionString)
		{
			return new SqlServer.SqlServerAtomicRepository<TModel>(connectionString);
		}
		protected override void AttemptToConnectAndOpen(string connectionString)
        {
			try
			{
				using SqlConnection connection = new SqlConnection(connectionString);
				connection.Open();
			}
			catch (Exception err)
			{
				throw new AtomsConnectionException("The connection to the database could not be established.", err);
			}
        }
    }
}
