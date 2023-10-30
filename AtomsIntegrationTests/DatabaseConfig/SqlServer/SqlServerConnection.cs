using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtomsIntegrationTests.DatabaseConfig.SqlServer
{
    internal static class SqlServerConnection
    {
        private static object _lock = new object();
        private static string connectionString = string.Empty;
        internal static string GetConnectionString()
        {
            lock (_lock)
            {
                if (connectionString == string.Empty)
                {
                    DotNetEnv.Env.Load("../../../");
                    connectionString =
                        Environment.GetEnvironmentVariable("INTEGRATION_TEST_SQL_SERVER_DB_CONNECTION_STRING") ?? throw new ArgumentNullException();
                }
                return connectionString;
            }
        }
    }
}
