using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ReLogic.Content;
using ReLogic.Content.Sources;
using Terraria.ModLoader.Core;

namespace Terraria.ModLoader.Assets
{
	public class ModContentSource : IContentSource, IDisposable
	{
		private readonly RejectedAssetCollection Rejections = new RejectedAssetCollection();

		private TeaFile file;

		public IContentValidator ContentValidator { get; set; }

		public ModContentSource(TeaFile file) {
			this.file = file ?? throw new ArgumentNullException(nameof(file));
		}

		public void ClearRejections() => Rejections.Clear();

		public void RejectAsset(string assetName, IRejectionReason reason) => Rejections.Reject(assetName, reason);

		public bool TryGetRejections(List<string> rejectionReasons) => Rejections.TryGetRejections(rejectionReasons);

		public bool HasAsset(string assetName) => file.HasFile(assetName);

		public string GetExtension(string assetName) => Path.GetExtension(assetName);

		public IEnumerable<string> EnumerateFiles() => file.Select(fileEntry => fileEntry.Name);

		public Stream OpenStream(string assetName) => new MemoryStream(file.GetBytes(assetName));

		public void Dispose() {
			file = null;

			ClearRejections();
		}
	}
}