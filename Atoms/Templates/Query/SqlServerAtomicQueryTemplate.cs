using Atoms.Exceptions;
using Atoms.Templates.Mutation;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Templates.Query
{
    internal class SqlServerAtomicQueryTemplate<T> : IAtomicQueryTemplate<T>
    {
		internal string ConnectionString { get; init; }
		internal string SqlText { get; init; }
        internal Func<Exception, Task>? ExceptionHandler { get; init; }

        public IAsyncEnumerable<T> QueryLazy(object? parameters = null)
        {
            return new AsyncEnumerableSqlDataReader(ConnectionString, SqlText, parameters);
        }

		private class AsyncEnumerableSqlDataReader : IAsyncEnumerable<T>
		{
			private string connectionString;
			private string sqlText;
			private object? parameters;

			public AsyncEnumerableSqlDataReader(string connectionString, string sqlText, object? parameters)
			{
				this.connectionString = connectionString;
				this.sqlText = sqlText;
				this.parameters = parameters;
			}

			public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
			{
				using SqlConnection connection = new SqlConnection(connectionString);
				connection.Open();
				using SqlCommand queryCommand = new SqlCommand(sqlText, connection);
				using var reader = await queryCommand.ExecuteReaderAsync();
				while (await reader.ReadAsync())
				{
					Type type = typeof(T);
					if (type.IsValueType && type.IsGenericType && (type.FullName?.StartsWith("System.ValueTuple") ?? false))
					{
						var valueTupleFields = type.GetFields().Where(f => f.Name.StartsWith("Item"));
						var valueTupleFieldTypes = valueTupleFields.Select(f => f.FieldType).ToArray();
						var valueTupleInstance = CreateValueTupleInstance(valueTupleFieldTypes);
						int i = 0;
						foreach (var field in valueTupleFields)
						{
							var value = reader[i++];
							if (field.FieldType.IsEnum && value is string valueString)
							{
								Enum.TryParse(field.FieldType, valueString, out object? parsedEnum);
								if (parsedEnum is not null)
									field.SetValue(valueTupleInstance, parsedEnum);
								else
									throw new EnumPropertyMappingFailedException();
							}
							else
							{
								field.SetValue(valueTupleInstance, value);
							}
						}
						yield return (T)valueTupleInstance;
					}
					else
					{
						var scalar = reader[0];
						yield return (T)scalar;
					}

				}
			}

			private object CreateValueTupleInstance(Type[] itemTypes)
			{
				Type valueTupleType = Type.GetType("System.ValueTuple`" + itemTypes.Length);
				valueTupleType = valueTupleType.MakeGenericType(itemTypes);
				// Create an instance of the tuple
				object tupleInstance = Activator.CreateInstance(valueTupleType);
				return tupleInstance;
			}

		}
	}
}
