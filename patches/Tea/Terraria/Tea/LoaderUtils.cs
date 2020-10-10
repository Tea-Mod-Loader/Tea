using System;

namespace Terraria.Tea
{
	internal class LoaderUtils
	{
		public static void ResetStaticMembers(Type type, bool recursive) {
			type.TypeInitializer?.Invoke(null, null);

			if (recursive) {
				foreach (var nestedType in type.GetNestedTypes()) {
					ResetStaticMembers(nestedType, recursive);
				}
			}
		}
	}
}
