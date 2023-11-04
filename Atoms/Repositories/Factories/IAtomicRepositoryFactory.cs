using Atoms.Exceptions;
using Atoms.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Repositories.Factories
{
    public interface IAtomicRepositoryFactory<TModel>
	{
        AtomicResult<IAtomicRepository<TModel>, AtomsException> CreateRepository(string connectionString);
    }
}
