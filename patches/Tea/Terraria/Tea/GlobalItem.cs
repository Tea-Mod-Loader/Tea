﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Terraria.Tea
{
	public class GlobalItem
	{
		public Mod mod { get; internal set; }

		public string Name { get; internal set; }

		public virtual bool Autoload(ref string name) => true;

		public virtual void SetDefaults(Item item) { }

		public virtual bool CanUseItem(Item item, Player player) => true;

		public virtual void UseStyle(Item item, Player player) { }

		public virtual void HoldStyle(Item item, Player player) { }

		public virtual void HoldItem(Item item, Player player) { }

		public virtual bool ConsumeAmmo(Item item, Player player) => true;

		public virtual bool Shoot(Item item, Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) => true;

		public virtual void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox) { }

		public virtual void MeleeEffects(Item item, Player player, Rectangle hitbox) { }

		public virtual void ModifyHitNPC(Item item, Player player, NPC target, ref int damage, ref float knockBack, ref bool crit) { }

		public virtual void OnHitNPC(Item item, Player player, NPC target, int damage, float knockBack, bool crit) { }

		public virtual void ModifyHitPvp(Item item, Player player, Player target, ref int damage, ref bool crit) { }

		public virtual void OnHitPvp(Item item, Player player, Player target, int damage, bool crit) { }

		public virtual bool UseItem(Item item, Player player) => false;

		public virtual bool ConsumeItem(Item item, Player player) => true;

		public virtual bool UseItemFrame(Item item, Player player) => false;

		public virtual bool HoldItemFrame(Item item, Player player) => false;

		public virtual void UpdateInventory(Item item, Player player) { }

		public virtual void UpdateEquip(Item item, Player player) { }

		public virtual void UpdateAccessory(Item item, Player player) { }

		public virtual string IsArmorSet(Item head, Item body, Item legs) => "";

		public virtual void UpdateArmorSet(Player player, string set) { }

		public virtual bool CanRightClick(Item item) => false;

		public virtual void RightClick(Item item, Player player) { }

		public virtual void DrawHair(Item item, ref bool drawHair, ref bool drawAltHair) { }

		public virtual bool DrawHead(Item item) => true;

		public virtual void VerticalWingSpeeds(Item item, ref float ascentWhenFalling, ref float ascentWhenRising,
			ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) { }

		public virtual void HorizontalWingSpeeds(Item item, ref float speed, ref float acceleration) { }

		public virtual void Update(Item item, ref float gravity, ref float maxFallSpeed) { } 

		public virtual Color? GetAlpha(Item item, Color lightColor) => null;

		public virtual bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale) => true;

		public virtual void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale) { }

		public virtual bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
			Color drawColor, Color itemColor, Vector2 origin, float scale) => true;

		public virtual void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
			Color drawColor, Color itemColor, Vector2 origin, float scale) { }

		public virtual bool CanEquipAccessory(Item item, Player player, int slot) => true;
	}
}
