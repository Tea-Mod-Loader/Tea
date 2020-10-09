using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.IO;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Terraria.Tea.UI
{
	public class UIModSourceItem : UIPanel
	{
		private string mod;
		private Texture2D dividerTexture;
		private UIText modName;

		public UIModSourceItem(string mod) {
			this.mod = mod;
			BorderColor = new Color(89, 116, 213) * 0.7f;
			dividerTexture = Main.Assets.Request<Texture2D>("Images/UI/Divider").Value;
			Height.Set(90f, 0f);
			Width.Set(0f, 1f);
			SetPadding(6f);
			modName = new UIText(Path.GetFileName(mod), 1f, false);
			modName.Left.Set(10f, 0f);
			modName.Top.Set(5f, 0f);
			Append(modName);

			UITextPanel<string> button = new UITextPanel<string>("Build", 1f, false);
			button.Width.Set(100f, 0f);
			button.Height.Set(30f, 0f);
			button.Left.Set(10f, 0f);
			button.Top.Set(40f, 0f);
			button.PaddingTop -= 2f;
			button.PaddingBottom -= 2f;
			button.OnMouseOver += new MouseEvent(FadedMouseOver);
			button.OnMouseOut += new MouseEvent(FadedMouseOut);
			button.OnClick += new MouseEvent(BuildMod);
			Append(button);

			UITextPanel<string> button2 = new UITextPanel<string>("Build + Reload", 1f, false);
			button2.CopyStyle(button);
			button2.Width.Set(200f, 0f);
			button2.Left.Set(150f, 0f);
			button2.OnMouseOver += new MouseEvent(FadedMouseOver);
			button2.OnMouseOut += new MouseEvent(FadedMouseOut);
			button2.OnClick += new MouseEvent(BuildAndReload);
			Append(button2);

			OnDoubleClick += new MouseEvent(BuildAndReload);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);

			CalculatedStyle innerDimensions = GetInnerDimensions();
			Vector2 drawPos = new Vector2(innerDimensions.X + 5f, innerDimensions.Y + 30f);

			spriteBatch.Draw(dividerTexture, drawPos, null, Color.White, 0f, Vector2.Zero, new Vector2((innerDimensions.Width - 10f) / 8f, 1f), SpriteEffects.None, 0f);
		}

		public override void MouseOver(UIMouseEvent evt) {
			base.MouseOver(evt);

			BackgroundColor = new Color(73, 94, 171);
			BorderColor = new Color(89, 116, 213);
		}

		public override void MouseOut(UIMouseEvent evt) {
			base.MouseOut(evt);

			BackgroundColor = new Color(63, 82, 151) * 0.7f;
			BorderColor = new Color(89, 116, 213) * 0.7f;
		}

		private static void FadedMouseOver(UIMouseEvent evt, UIElement listeningElement) {
			SoundEngine.PlaySound(12, -1, -1, 1);

			((UIPanel)evt.Target).BackgroundColor = new Color(73, 94, 171);
		}

		private static void FadedMouseOut(UIMouseEvent evt, UIElement listeningElement) => ((UIPanel)evt.Target).BackgroundColor = new Color(63, 82, 151) * 0.7f;

		private void BuildMod(UIMouseEvent evt, UIElement listeningElement) {
			SoundEngine.PlaySound(10, -1, -1, 1);

			Loader.modToBuild = mod;
			Loader.reloadAfterBuild = false;
			Loader.buildAll = false;
			Main.menuMode = LoaderMenus.buildModID;
		}

		private void BuildAndReload(UIMouseEvent evt, UIElement listeningElement) {
			SoundEngine.PlaySound(10, -1, -1, 1);

			Loader.modToBuild = mod;
			Loader.reloadAfterBuild = true;
			Loader.buildAll = false;
			Main.menuMode = LoaderMenus.buildModID;
		}
	}
}
