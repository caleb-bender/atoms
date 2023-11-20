using CalebBender.Atoms.Exceptions;
using CalebBender.Atoms.Repositories.SqlServer.SqlGeneration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalebBender.Atoms.Repositories.SqlServer
{
	internal static class SqlServerErrorTranslators
	{
		internal static void TranslateDuplicatePrimaryKeyOrIndexError(SqlException err, Type modelType)
		{
			if (err.Number == 2627 || err.Number == 2601)
				throw new DuplicateUniqueIdException($"The creation of a duplicate unique id for the entity \"{modelType.Name}\" was attempted.", err);
		}

		internal static void TranslateOperandTypeClashError(SqlException err, Type modelType)
		{
			if (err.Number == 206)
				throw new ModelPropertyTypeMismatchException(
					$"One or more properties' types in the model \"{modelType.Name}\" are incompatible with the corresponding database property or properties. Ensure that each model property is mapped to the correct database property and contains a compatible type.", err
				);
		}

		internal static void TranslateInvalidColumnNameError(SqlException err, Type modelType)
		{
			if (err.Number == 207)
				throw new ModelDbEntityMismatchException(
					$@"The schema of the model ""{modelType.Name}"" does not match its corresponding database entity schema.
					Make sure the name of the model and its public properties match the database entity schema.
					If the model name or any of its properties' names don't match, consider manually defining names by annotating
					with DbEntityNameAttribute or DbPropertyNameAttribute respectively.", err);
		}

		internal static void TranslateInvalidObjectNameError(SqlException err, Type modelType)
		{
			if (err.Number == 208)
				throw new DbEntityNotFoundException($@"The database entity for the model ""{modelType.Name}"" was not found.
					Make sure the plural of the model name matches the name of the database entity name, or annotate the model
					definition using the DbEntityNameAttribute to provide the name manually.", err);
		}
	}
}
