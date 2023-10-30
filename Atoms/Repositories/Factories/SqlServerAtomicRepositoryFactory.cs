using Atoms.Exceptions;
using Atoms.Utils;
using System.Data.SqlClient;

using AtomicRepositoryResult = Atoms.Utils.AtomicResult<Atoms.Repositories.IAtomicRepository, Atoms.Exceptions.AtomsConnectionException>;

namespace Atoms.Repositories.Factories
{
    public class SqlServerAtomicRepositoryFactory : IAtomicRepositoryFactory
    {
        public AtomicRepositoryResult CreateRepository(string connectionString)
        {
            try
            {
                AttemptToConnectAndOpen(connectionString);
                return new AtomicRepositoryResult.Ok(new SqlServer.SqlServerAtomicRepository(connectionString));
            }
            catch (Exception err)
            {
                var connectionException = new AtomsConnectionException(err.Message, err);
                return new AtomicRepositoryResult.Error(connectionException);
            }
        }

        private static void AttemptToConnectAndOpen(string connectionString)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
        }
    }
}
