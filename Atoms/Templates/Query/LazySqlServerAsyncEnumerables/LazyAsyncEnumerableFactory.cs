using CalebBender.Atoms.Utils.Reflection.Scalars;
using CalebBender.Atoms.Utils.Reflection.Tuples;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalebBender.Atoms.Templates.Query.LazySqlServerAsyncEnumerables
{
	internal class LazyAsyncEnumerableFactory<T>
	{
		public LazyAsyncEnumerable<T> Create(
			string connectionString, string sqlText, object? parameters, Func<Exception, Task>? exceptionHandler, CancellationToken cancellationToken
		)
		{
			if (ValueTupleHelpers<T>.IsValueTuple)
				return new ValueTupleAsyncEnumerable<T>(connectionString, sqlText, parameters, exceptionHandler, cancellationToken);
			else if (ScalarHelpers<T>.IsScalar)
				return new ScalarAsyncEnumerable<T>(connectionString, sqlText, parameters, exceptionHandler, cancellationToken);
			else if (IsValidModelType())
				return new DataModelAsyncEnumerable<T>(connectionString, sqlText, parameters, exceptionHandler, cancellationToken);
			else
				throw new InvalidOperationException($"The type \"{typeof(T).Name}\" is not a valid data model class. It must be a class with a parameterless constructor.");
		}

		public static bool IsValidModelType()
		{
			Type type = typeof(T);

			// Check if T is a class
			bool isClass = type.IsClass;

			// Check if T has a parameterless constructor
			bool hasDefaultConstructor = type.GetConstructor(Type.EmptyTypes) != null;

			return isClass && hasDefaultConstructor;
		}
	}
}
