using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Utils.Reflection
{
	internal class AttributeCheckerHelpers
	{
		internal static bool HasAttributeOnAtLeastOneProperty<TAttribute>(Type type) where TAttribute : Attribute
		{
			// Get all public instance properties.
			PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
			foreach (PropertyInfo property in properties)
			{
				// Check if the TAttribute is defined on the property.
				if (Attribute.IsDefined(property, typeof(TAttribute))) return true;
			}
			return false;
		}
	}
}
