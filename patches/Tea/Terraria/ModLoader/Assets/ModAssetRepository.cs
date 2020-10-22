using ReLogic.Content;
using ReLogic.Content.Readers;
using ReLogic.Content.Sources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Terraria.ModLoader.Assets
{
	public class ModAssetRepository : AssetRepository
	{
		private class ContentTypeInfo
		{
			public readonly HashSet<string> FilePaths = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
			public readonly Dictionary<string, string> PathAssociations = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
		}

		private readonly AssetReaderCollection AssetReaderCollection;
		private readonly Dictionary<Type, ContentTypeInfo> PerContentTypeInfo = new Dictionary<Type, ContentTypeInfo>();

		public ModAssetRepository(AssetReaderCollection assetReaderCollection, IAssetLoader syncLoader, IAsyncAssetLoader asyncLoader, IEnumerable<IContentSource> sources = null) : base(syncLoader, asyncLoader) {
			AssetReaderCollection = assetReaderCollection;

			if (sources != null) {
				SetSources(sources, AssetRequestMode.DoNotLoad);
			}
		}

		public override void SetSources(IEnumerable<IContentSource> sources, AssetRequestMode mode = AssetRequestMode.ImmediateLoad) {
			base.SetSources(sources, mode);

			FillPathCache();
		}

		public override Asset<T> Request<T>(string assetName, AssetRequestMode mode = AssetRequestMode.ImmediateLoad) {
			if (PerContentTypeInfo.TryGetValue(typeof(T), out ContentTypeInfo info) && info.PathAssociations.TryGetValue(assetName, out string guessedName)) {
				assetName = guessedName;
			}

			return base.Request<T>(assetName, mode);
		}

		public bool HasAsset<T>(string assetName) where T : class => PerContentTypeInfo.TryGetValue(typeof(T), out ContentTypeInfo info) && (info.PathAssociations.ContainsKey(assetName) || info.FilePaths.Contains(assetName));

		public IEnumerable<string> EnumeratePaths<T>() where T : class {
			if (PerContentTypeInfo.TryGetValue(typeof(T), out ContentTypeInfo info)) {
				return info.FilePaths;
			}

			return Enumerable.Empty<string>();
		}

		public IEnumerable<Asset<T>> EnumerateLoadedAssets<T>() where T : class {
			foreach (KeyValuePair<string, IAsset> pair in _assets) {
				IAsset asset = pair.Value;

				if (asset.IsLoaded && asset is Asset<T> result) {
					yield return result;
				}
			}
		}

		public Asset<T> CreateAsset<T>(string assetName, T content) where T : class => CreateAsset(assetName, content, true);

		internal Asset<T> CreateUntrackedAsset<T>(T content) where T : class => CreateAsset(string.Empty, content, true);

		internal Asset<T> CreateUntrackedAsset<T>(string assetName, T content) where T : class => CreateAsset(assetName, content, true);

		private Asset<T> CreateAsset<T>(string assetName, T content, bool tracked) where T : class {
			Type type = typeof(Asset<T>);
			Asset<T> asset = (Asset<T>)Activator.CreateInstance(type, BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance, null, new object[] { assetName }, null);

			type.GetMethod("SubmitLoadedContent", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(asset, new object[] { content, _sources.First() });

			if (tracked) {
				if (_assets.TryGetValue(assetName, out IAsset oldAsset)) {
					oldAsset?.Dispose();
				}

				_assets[assetName] = asset;
			}

			return asset;
		}

		private void FillPathCache() {
			PerContentTypeInfo.Clear();

			foreach (IContentSource source in _sources) {
				foreach (string path in source.EnumerateFiles()) {
					string extension = Path.GetExtension(path)?.ToLower();

					if (extension == null) {
						continue;
					}

					string pathWithoutExtension = Path.ChangeExtension(path, null);

					if (!AssetReaderCollection.TryGetReader(extension, out IAssetReader assetReader)) {
						continue;
					}

					Type[] types = assetReader.GetAssociatedTypes();

					if (types == null || types.Length == 0) {
						continue;
					}

					foreach (Type type in types) {
						if (!PerContentTypeInfo.TryGetValue(type, out ContentTypeInfo info)) {
							PerContentTypeInfo[type] = info = new ContentTypeInfo();
						}

						info.FilePaths.Add(path);

						info.PathAssociations[pathWithoutExtension] = info.PathAssociations.ContainsKey(pathWithoutExtension) ? null : path;
					}
				}
			}
		}

		private string FilterPath(Type contentType, string path) {
			if (PerContentTypeInfo.TryGetValue(contentType, out var info)) {
				return path;
			}

			if (!info.PathAssociations.TryGetValue(path, out string pathAssociation)) {
				return path;
			}

			return pathAssociation ?? throw new ArgumentException($"Extensionless path '{path}' is ambiguous. Please provide an extension.");
		}
	}
}