using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using System;
using System.Reflection;
using Terraria.Initializers;
using Terraria.ModLoader.Assets;

namespace Terraria.ModLoader
{
	public static class ModLoader
	{
		public static Version version = new Version(0, 1, 0, 0);
		public static readonly string branchName = "tea-revamp";
		public static readonly int beta = 1;
		public static readonly string versionedName = $"Tea v{version}" + (branchName.Length == 0 ? "" : $" {branchName}") + (beta == 0 ? "" : $" Beta {beta}");

		public static readonly string versionTag = $"v{version}" + (branchName.Length == 0 ? "" : $"-{branchName.ToLower()}") + (beta == 0 ? "" : $"-beta{beta}");

		internal static ModAssetRepository ManifestAssets { get; set; }
		internal static AssemblyResourcesContentSource ManifestContentSource { get; set; }

		internal static void PrepareAssets() {
			if (Main.dedServ) {
				return;
			}

			AssetReaderCollection assetReaderCollection = AssetInitializer.assetReaderCollection;

			AsyncAssetLoader asyncAssetLoader = new AsyncAssetLoader(assetReaderCollection, 20);
			asyncAssetLoader.RequireTypeCreationOnTransfer(typeof(Texture2D));
			asyncAssetLoader.RequireTypeCreationOnTransfer(typeof(DynamicSpriteFont));
			asyncAssetLoader.RequireTypeCreationOnTransfer(typeof(SpriteFont));

			AssetLoader assetLoader = new AssetLoader(assetReaderCollection);

			ManifestContentSource = new AssemblyResourcesContentSource(Assembly.GetExecutingAssembly());
			ManifestAssets = new ModAssetRepository(assetReaderCollection, assetLoader, asyncAssetLoader, new[] { ManifestContentSource });
		}
	}
}
