using Atoms.DataAttributes;
using Atoms.Exceptions;
using Atoms.Utils;
using System.Data.SqlClient;

namespace Atoms.Repositories.Factories
{
    public class SqlServerAtomicRepositoryFactory<TModel> : AtomicRepositoryFactory<TModel>
		where TModel : class, new()
	{
		protected override AtomicResult<IAtomicRepository<TModel>, AtomsException> NewAtomicRepositoryResult(string connectionString)
		{
			return new AtomicResult<IAtomicRepository<TModel>, AtomsException>
				.Ok(new SqlServer.SqlServerAtomicRepository<TModel>(connectionString));
		}
		protected override void AttemptToConnectAndOpen(string connectionString)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
        }
    }
}
