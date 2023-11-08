using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Utils.Reflection
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
	}
}
