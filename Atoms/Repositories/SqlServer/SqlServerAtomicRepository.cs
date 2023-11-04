using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Repositories.SqlServer
{
    internal class SqlServerAtomicRepository<TModel> : IAtomicRepository<TModel>
    {
        private string connectionString;

        internal SqlServerAtomicRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }
    }
}
