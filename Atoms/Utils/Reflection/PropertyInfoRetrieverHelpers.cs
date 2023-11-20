using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CalebBender.Atoms.Utils.Reflection
{
	internal static class PropertyInfoRetrieverHelpers
	{
		internal static IEnumerable<PropertyInfo> GetAllPublicProperties(Type type)
		{
			// Get all public instance properties.
			PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
			return properties;
		}
	}
}
