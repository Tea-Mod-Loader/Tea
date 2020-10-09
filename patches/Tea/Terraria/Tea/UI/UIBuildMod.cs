using System.IO;
using Terraria.UI;

namespace Terraria.Tea.UI
{
	public class UIBuildMod : UIState
	{
		private UILoadProgress loadProgress;

		public override void OnInitialize() {
			loadProgress = new UILoadProgress();
			loadProgress.Width.Set(0f, 0.8f);
			loadProgress.MaxWidth.Set(600f, 0f);
			loadProgress.Height.Set(150f, 0f);
			loadProgress.HAlign = 0.5f;
			loadProgress.VAlign = 0.5f;
			loadProgress.Top.Set(10f, 0f);
			Append(loadProgress);
		}

		internal void SetProgress(int num, int max) => loadProgress.SetProgress(num / max);

		internal void SetReading() => loadProgress.SetText("Reading Properties: " + Path.GetFileName(Loader.modToBuild));

		internal void SetCompiling() => loadProgress.SetText("Compiling " + Path.GetFileName(Loader.modToBuild) + "...");

		internal void SetBuildText() => loadProgress.SetText("Building " + Path.GetFileName(Loader.modToBuild) + "...");
	}
}
