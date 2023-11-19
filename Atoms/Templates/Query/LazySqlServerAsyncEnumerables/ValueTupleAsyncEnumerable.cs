using Atoms.Exceptions;
using Atoms.Utils.Reflection.Tuples;
using Atoms.Utils.Reflection.TypeMapping;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using static Atoms.Utils.Reflection.TypeMapping.EnumMappingHelpers;

namespace Atoms.Templates.Query.LazySqlServerAsyncEnumerables
{
	internal class ValueTupleAsyncEnumerable<T> : LazyAsyncEnumerable<T>
	{
		public ValueTupleAsyncEnumerable(
			string connectionString,
			string sqlText,
			object? parameters
		) : base(connectionString, sqlText, parameters)
		{
			if (!ValueTupleHelpers<T>.IsValueTuple)
				throw new ArgumentException($"The generic type argument \"{typeof(T).Name}\" is not a ValueTuple");
		}

		protected override T GetValueFromReader(SqlDataReader reader)
		{
			var valueTupleInstance = ValueTupleHelpers<T>.CreateInstance();
			int i = 0;
			foreach (var field in ValueTupleHelpers<T>.Fields)
			{
				var value = reader[i++];
				var (parsingStatus, parsedEnum) = AttemptToParseEnumField(field, value);
				field.SetValue(valueTupleInstance, parsedEnum);
			}
			return (T)valueTupleInstance;
		}
	}
}
