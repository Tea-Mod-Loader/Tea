using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Terraria.Tea.UI
{
	public class UIColorTextPanel : UIPanel
	{
		private string _text = "";
		private Color _color;
		private float _textScale = 1f;
		private Vector2 _textSize = Vector2.Zero;
		private bool _isLarge;

		public UIColorTextPanel(string text, Color color, float textScale = 1f, bool large = false) {
			SetText(text, textScale, large);
			SetColor(color);
		}

		public override void Recalculate() {
			SetText(_text, _textScale, _isLarge);
			base.Recalculate();
		}

		public void SetText(string text, float textScale, bool large) {
			DynamicSpriteFont spriteFont = large ? FontAssets.DeathText.Value : FontAssets.MouseText.Value;
			Vector2 textSize = new Vector2(spriteFont.MeasureString(text).X, large ? 32f : 16f) * textScale;

			_text = text;
			_textScale = textScale;
			_textSize = textSize;
			_isLarge = large;

			MinWidth.Set(textSize.X + PaddingLeft + PaddingRight, 0f);
			MinHeight.Set(textSize.Y + PaddingTop + PaddingBottom, 0f);
		}

		public void SetColor(Color color) => _color = color;

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);

			CalculatedStyle innerDimensions = GetInnerDimensions();
			Vector2 pos = innerDimensions.Position();

			if (_isLarge) {
				pos.Y -= 10f * _textScale;
			}
			else {
				pos.Y -= 2f * _textScale;
			}

			pos.X += (innerDimensions.Width - _textSize.X) * 0.5f;

			if (_isLarge) {
				Utils.DrawBorderStringBig(spriteBatch, _text, pos, _color, _textScale, 0f, 0f, -1);

				return;
			}

			Utils.DrawBorderString(spriteBatch, _text, pos, _color, _textScale, 0f, 0f, -1);
		}
	}
}
