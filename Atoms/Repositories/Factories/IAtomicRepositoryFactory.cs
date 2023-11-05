using Atoms.Exceptions;
using Atoms.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Repositories.Factories
{
    public interface IAtomicRepositoryFactory<TModel> where TModel : class, new()
	{
        /// <summary>
        /// Attempts to create an IAtomicRepository implementation given TModel and the dbConnectionString
        /// </summary>
        /// <param name="dbConnectionString"></param>
        /// <returns>The instantiated IAtomicRepository implementation</returns>
        /// <exception cref="MissingUniqueIdAttributeException"></exception>
        /// <exception cref="AtomsConnectionException"></exception>
        AtomicResult<IAtomicRepository<TModel>, AtomsException> CreateRepository(string dbConnectionString);
    }
}
