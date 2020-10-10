using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria.DataStructures;

namespace Terraria.Tea
{
	public class ModItem
	{
		public Item item { get; internal set; }

		public Mod mod { get; internal set; }

		internal string texture;

		public ModItem() {
			item = new Item();
			item.modItem = this;
		}

		public virtual bool Autoload(ref string name, ref string texture /*, TODO: IList<EquipType> equips*/) => true;

		// TODO: public virtual void AutoloadEquip(EquipType equip, ref string texture, ref string armTexture, ref string femaleTexture) { }

		public virtual DrawAnimation GetAnimation() => null;

		public virtual void SetDefaults() { }

		public virtual bool CanUseItem(Player player) => true;

		public virtual void UseStyle(Player player) { }

		public virtual void HoldStyle(Player player) { }

		public virtual void HoldItem(Player player) { }

		public virtual bool ConsumeAmmo(Player player) => true;

		public virtual bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) => true;

		public virtual void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox) { }

		public virtual void MeleeEffects(Player player, Rectangle hitbox) { }

		public virtual void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit) { }

		public virtual void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit) { }

		public virtual void ModifyHitPvp(Player player, Player target, ref int damage, ref bool crit) { }

		public virtual void OnHitPvp(Player player, Player target, int damage, bool crit) { }

		public virtual bool UseItem(Player player) => false;

		public virtual bool ConsumeItem(Player player) => true;

		public virtual bool UseItemFrame(Player player) => false;

		public virtual bool HoldItemFrame(Player player) => false;

		public virtual void UpdateInventory(Player player) { }

		public virtual void UpdateEquip(Player player) { }

		public virtual void UpdateAccessory(Player player) { }

		public virtual bool IsArmorSet(Item head, Item body, Item legs) => false;

		public virtual void UpdateArmorSet(Player player) { }

		public virtual bool CanRightClick() => false;

		public virtual void RightClick(Player player) { }

		public virtual void DrawHair(ref bool drawHair, ref bool drawAltHair) { }

		public virtual bool DrawHead() => true;

		public virtual void VerticalWingSpeeds(ref float ascentWhenFalling, ref float ascentWhenRising,
			ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) { }

		public virtual void HorizontalWingSpeeds(ref float speed, ref float acceleration) { }

		public virtual void Update(ref float gravity, ref float maxFallSpeed) { }

		public virtual Color? GetAlpha(Color lightColor) => null;

		public virtual bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale) => true;

		public virtual void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale) { }

		public virtual bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor,
			Color itemColor, Vector2 origin, float scale) => true;

		public virtual void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor,
			Color itemColor, Vector2 origin, float scale) { }

		public virtual bool CanEquipAccessory(Player player, int slot) => true;

		internal void SetupItem(Item item) {
			SetupModItem(item);
			// TODO: EquipLoader.SetSlot(item);
			item.modItem.SetDefaults();
		}

		internal void SetupModItem(Item item) {
			ModItem newItem = (ModItem)Activator.CreateInstance(GetType());
			newItem.item = item;
			item.modItem = newItem;
			newItem.mod = mod;
		}

		public virtual void SaveCustomData(BinaryWriter writer) { }

		public virtual void LoadCustomData(BinaryReader reader) { }

		public virtual void AddRecipes() { }
	}
}