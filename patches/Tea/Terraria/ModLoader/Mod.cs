using Terraria.ModLoader.Assets;

namespace Terraria.ModLoader
{
	public class Mod
	{
		public ModAssetRepository Assets { get; private set; }
		public ModContentSource ContentSource { get; private set; }
	}
}
