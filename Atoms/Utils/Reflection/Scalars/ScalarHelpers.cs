namespace Atoms.Utils.Reflection.Scalars
{
	internal static class ScalarHelpers<T>
	{
		private static readonly bool isScalar = GetIsScalar();
		internal static bool IsScalar { get => isScalar; }

		private static bool GetIsScalar()
		{
			var type = typeof(T);
			return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(DateTime)
				|| type == typeof(Guid);
		}
	}
}
