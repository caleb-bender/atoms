using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Utils.Reflection.Tuples
{
	internal static class ValueTupleHelpers<T>
	{
		private static Type type = typeof(T);
		private static readonly IEnumerable<FieldInfo> valueTupleFields = type.GetFields().Where(f => f.Name.StartsWith("Item"));
		private static readonly Type[] valueTupleFieldTypes = valueTupleFields.Select(f => f.FieldType).ToArray();
		private static readonly bool isValueTuple = GetIsValueTuple();
		internal static bool IsValueTuple { get => isValueTuple; }
		internal static IEnumerable<FieldInfo> Fields { get => valueTupleFields; }

		internal static object CreateInstance()
		{
			Type valueTupleType = Type.GetType("System.ValueTuple`" + valueTupleFieldTypes.Length);
			valueTupleType = valueTupleType.MakeGenericType(valueTupleFieldTypes);
			// Create an instance of the tuple
			object tupleInstance = Activator.CreateInstance(valueTupleType);
			return tupleInstance;
		}

		private static bool GetIsValueTuple()
		{
			return type.IsValueType && type.IsGenericType &&
				(type.FullName?.StartsWith("System.ValueTuple") ?? false);
		}
	}
}
