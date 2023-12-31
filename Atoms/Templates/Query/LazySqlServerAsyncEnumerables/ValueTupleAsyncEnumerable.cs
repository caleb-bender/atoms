﻿using CalebBender.Atoms.Exceptions;
using CalebBender.Atoms.Utils.Reflection.Tuples;
using CalebBender.Atoms.Utils.Reflection.TypeMapping;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using static CalebBender.Atoms.Utils.Reflection.TypeMapping.EnumMappingHelpers;

namespace CalebBender.Atoms.Templates.Query.LazySqlServerAsyncEnumerables
{
	internal class ValueTupleAsyncEnumerable<T> : LazyAsyncEnumerable<T>
	{
		public ValueTupleAsyncEnumerable(
			string connectionString,
			string sqlText,
			object? parameters,
			Func<Exception, Task>? exceptionHandler,
			CancellationToken cancellationToken
		) : base(connectionString, sqlText, parameters, exceptionHandler, cancellationToken)
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
				var (parsingStatus, parsedEnumOrValue) = AttemptToParseEnumField(field, value);
				var parsedValue = (parsedEnumOrValue == DBNull.Value) ? null : parsedEnumOrValue;
				field.SetValue(valueTupleInstance, parsedValue);
			}
			return (T)valueTupleInstance;
		}
	}
}
