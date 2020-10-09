using Terraria.Audio;
using Terraria.Tea.UI;

namespace Terraria.Tea
{
	public class LoaderMenus
	{
		public static readonly int modsMenuID = 100001;
		public static readonly int modSourcesID = 100002;
		public static readonly int loadModsID = 100003;
		public static readonly int buildModID = 100004;
		public static readonly int buildAllModsID = 100005;
		public static readonly int errorMessageID = 100006;
		public static readonly int reloadModsID = 100007;

		internal static UILoadMods loadMods = new UILoadMods();
		internal static UIBuildMod buildMod = new UIBuildMod();
		internal static UIErrorMessage errorMessage = new UIErrorMessage();

		private static UIMods modsMenu = new UIMods();
		private static UIModSources modSources = new UIModSources();

		internal static void TeaMenus(Main main, int selectedMenu, string[] buttonNames, float[] buttonScales, ref int offY, ref int spacing, ref int numButtons) {
			if (Main.menuMode == modsMenuID) {
				Main.MenuUI.SetState(modsMenu);

				Main.menuMode = 888;
			}
			else if (Main.menuMode == modSourcesID) {
				Main.MenuUI.SetState(modSources);

				Main.menuMode = 888;
			}
			else if (Main.menuMode == loadModsID) {
				Main.MenuUI.SetState(loadMods);

				Main.menuMode = 888;

				Loader.Load();
			}
			else if (Main.menuMode == buildModID) {
				Main.MenuUI.SetState(buildMod);

				Main.menuMode = 888;

				Loader.BuildMod();
			}
			else if (Main.menuMode == buildAllModsID) {
				Main.MenuUI.SetState(buildMod);

				Main.menuMode = 888;

				Loader.BuildAllMods();
			}
			else if (Main.menuMode == errorMessageID) {
				Main.MenuUI.SetState(errorMessage);

				Main.menuMode = 888;
			}
			else if (Main.menuMode == reloadModsID) {
				Loader.Reload();
			}
		}
	}
}
