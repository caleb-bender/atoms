using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalebBender.Atoms.Utils.Reflection
{
	internal static class TypeCheckingHelpers
	{
		private readonly static HashSet<string> compatibleNonPrimitiveTypesForUniqueId = new HashSet<string> {
			typeof(DateTime).Name,
			typeof(Guid).Name,
			typeof(string).Name
		};

		internal static bool IsCompatibleUniqueIdType(Type type)
		{
			return type.IsPrimitive || compatibleNonPrimitiveTypesForUniqueId.Contains(type.Name) || type.IsEnum;
		}

		internal static (bool, IEnumerable) IsIEnumerable(object obj)
		{
			var objType = obj.GetType();
			if (objType.IsClass && !objType.IsValueType && objType != typeof(string) && obj is IEnumerable enumerable)
				return (true, enumerable);
			return (false, null);
		}
	}
}
