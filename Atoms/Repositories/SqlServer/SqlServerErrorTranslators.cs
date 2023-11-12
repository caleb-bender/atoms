using Atoms.Exceptions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Repositories.SqlServer
{
	internal static class SqlServerErrorTranslators
	{
		internal static void TranslateDuplicatePrimaryKeyOrIndexError(SqlException err, Type modelType)
		{
			if (err.Number == 2627 || err.Number == 2601)
				throw new DuplicateUniqueIdException($"The creation of a duplicate unique id for the entity \"{modelType.Name}\" was attempted.", err);
		}
	}
}
