using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.Tea
{
	public static class ItemLoader
	{
		private static int nextItem = ItemID.Count;

		internal static readonly IDictionary<int, ModItem> items = new Dictionary<int, ModItem>();
		internal static readonly IList<GlobalItem> globalItems = new List<GlobalItem>();
		internal static readonly IList<int> animations = new List<int>();
		internal static readonly int[] vanillaWings = new int[Main.maxWings];

		public static int Count = nextItem;
		public static Texture2D[] itemTexture = new Texture2D[ItemID.Count];

		internal static int ReserveItemID() {
			int reserveID = nextItem;
			nextItem++;

			return reserveID;
		}

		public static ModItem GetItem(int type) {
			if (items.ContainsKey(type)) {
				return items[type];
			}
			else {
				return null;
			}
		}

		internal static void ResizeArrays() {
            Array.Resize(ref itemTexture, nextItem);
			Array.Resize(ref TextureAssets.Item, nextItem);
			Array.Resize(ref TextureAssets.ItemFlame, nextItem);
			Array.Resize(ref Main.itemAnimations, nextItem);
			Array.Resize(ref Item.cachedItemSpawnsByType, nextItem);
			Array.Resize(ref Item.staff, nextItem);
			Array.Resize(ref Item.claw, nextItem);
			Array.Resize(ref Lang._itemNameCache, nextItem);
			Array.Resize(ref Lang._itemTooltipCache, nextItem);
            Array.Resize(ref ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn, nextItem);
            Array.Resize(ref ItemID.Sets.NewItemSpawnPriority, nextItem);
            Array.Resize(ref ItemID.Sets.CanBeQuickusedOnGamepad, nextItem);
            Array.Resize(ref ItemID.Sets.ForcesBreaksSleeping, nextItem);
            Array.Resize(ref ItemID.Sets.SkipsInitialUseSound, nextItem);
            Array.Resize(ref ItemID.Sets.UsesCursedByPlanteraTooltip, nextItem);
            Array.Resize(ref ItemID.Sets.IsAKite, nextItem);
            Array.Resize(ref ItemID.Sets.ForceConsumption, nextItem);
            Array.Resize(ref ItemID.Sets.HasAProjectileThatHasAUsabilityCheck, nextItem);
            Array.Resize(ref ItemID.Sets.CanGetPrefixes, nextItem);
            Array.Resize(ref ItemID.Sets.ColorfulDyeValues, nextItem);
            Array.Resize(ref ItemID.Sets.flowerPacketInfo, nextItem);
            Array.Resize(ref ItemID.Sets.IsAMaterial, nextItem);
            Array.Resize(ref ItemID.Sets.IgnoresEncumberingStone, nextItem);
            Array.Resize(ref ItemID.Sets.ToolTipDamageMultiplier, nextItem);
            Array.Resize(ref ItemID.Sets.IsAPickup, nextItem);
            Array.Resize(ref ItemID.Sets.IsDrill, nextItem);
            Array.Resize(ref ItemID.Sets.IsChainsaw, nextItem);
			Array.Resize(ref ItemID.Sets.IsPaintScraper, nextItem);
			Array.Resize(ref ItemID.Sets.IsFood, nextItem);
			Array.Resize(ref ItemID.Sets.FoodParticleColors, nextItem);
            Array.Resize(ref ItemID.Sets.DrinkParticleColors, nextItem);
            Array.Resize(ref ItemID.Sets.BannerStrength, nextItem);
            Array.Resize(ref ItemID.Sets.KillsToBanner, nextItem);
            Array.Resize(ref ItemID.Sets.CanFishInLava, nextItem);
            Array.Resize(ref ItemID.Sets.IsLavaBait, nextItem);
            Array.Resize(ref ItemID.Sets.ItemSpawnDecaySpeed, nextItem);
            Array.Resize(ref ItemID.Sets.IsFishingCrate, nextItem);
            Array.Resize(ref ItemID.Sets.IsFishingCrateHardmode, nextItem);
            Array.Resize(ref ItemID.Sets.CanBePlacedOnWeaponRacks, nextItem);
            Array.Resize(ref ItemID.Sets.TextureCopyLoad, nextItem);
            Array.Resize(ref ItemID.Sets.TrapSigned, nextItem);
            Array.Resize(ref ItemID.Sets.Deprecated, nextItem);
            Array.Resize(ref ItemID.Sets.CommonCoin, nextItem);
            Array.Resize(ref ItemID.Sets.ItemIconPulse, nextItem);
            Array.Resize(ref ItemID.Sets.ItemNoGravity, nextItem);
            Array.Resize(ref ItemID.Sets.ExtractinatorMode, nextItem);
            Array.Resize(ref ItemID.Sets.StaffMinionSlotsRequired, nextItem);
            Array.Resize(ref ItemID.Sets.ExoticPlantsForDyeTrade, nextItem);
            Array.Resize(ref ItemID.Sets.NebulaPickup, nextItem);
            Array.Resize(ref ItemID.Sets.AnimatesAsSoul, nextItem);
            Array.Resize(ref ItemID.Sets.gunProj, nextItem);
            Array.Resize(ref ItemID.Sets.SortingPriorityBossSpawns, nextItem);
            Array.Resize(ref ItemID.Sets.NeverAppearsAsNewInInventory, nextItem);
            Array.Resize(ref ItemID.Sets.SortingPriorityWiring, nextItem);
            Array.Resize(ref ItemID.Sets.SortingPriorityMaterials, nextItem);
            Array.Resize(ref ItemID.Sets.SortingPriorityExtractibles, nextItem);
            Array.Resize(ref ItemID.Sets.SortingPriorityRopes, nextItem);
            Array.Resize(ref ItemID.Sets.SortingPriorityPainting, nextItem);
            Array.Resize(ref ItemID.Sets.SortingPriorityTerraforming, nextItem);
            Array.Resize(ref ItemID.Sets.GamepadExtraRange, nextItem);
            Array.Resize(ref ItemID.Sets.GamepadWholeScreenUseRange, nextItem);
            Array.Resize(ref ItemID.Sets.BonusMeleeSpeedMultiplier, nextItem);
            Array.Resize(ref ItemID.Sets.GamepadSmartQuickReach, nextItem);
            Array.Resize(ref ItemID.Sets.Yoyo, nextItem);
            Array.Resize(ref ItemID.Sets.AlsoABuildingItem, nextItem);
            Array.Resize(ref ItemID.Sets.LockOnIgnoresCollision, nextItem);
            Array.Resize(ref ItemID.Sets.LockOnAimAbove, nextItem);
            Array.Resize(ref ItemID.Sets.LockOnAimCompensation, nextItem);
            Array.Resize(ref ItemID.Sets.SingleUseInGamepad, nextItem);
            Array.Resize(ref ItemID.Sets.Torches, nextItem);
            Array.Resize(ref ItemID.Sets.WaterTorches, nextItem);
            Array.Resize(ref ItemID.Sets.Workbenches, nextItem);
			Array.Resize(ref ItemID.Sets.ItemsThatAllowRepeatedRightClick, nextItem);

			for (int k = ItemID.Count; k < nextItem; k++) {
				Lang._itemNameCache[k] = LocalizedText.Empty;
				Lang._itemTooltipCache[k] = ItemTooltip.None;
				Item.cachedItemSpawnsByType[k] = -1;
			}

			Array.Resize(ref Main.anglerQuestItemNetIDs, 41);

			if (vanillaWings[1] != 0)
				return;

			Item item = new Item();

			for (int k = 0; k < ItemID.Count; k++) {
				item.SetDefaults(k);

				if (item.wingSlot > 0) {
					vanillaWings[item.wingSlot] = k;
				}
			}
		}

		public static bool IsModItem(Item item) => item.type >= ItemID.Count;

		internal static void WriteID(Item item, BinaryWriter writer) {
			if (item.netID >= ItemID.Count) {
				int writeID = Int32.MaxValue;
				byte[] data;

				using (MemoryStream memoryStream = new MemoryStream()) {
					using (BinaryWriter customWriter = new BinaryWriter(memoryStream)) {
						item.modItem.SaveCustomData(customWriter);
						customWriter.Flush();
						data = memoryStream.ToArray();
					}
				}

				if (data.Length > 0) {
					writeID -= 1;
				}

				writer.Write(writeID);
				writer.Write(item.modItem.mod.Name);
				writer.Write(Lang._itemNameCache[item.type].Value);

				if (data.Length > 0) {
					writer.Write((short)data.Length);
					writer.Write(data);
				}
			}
			else {
				writer.Write(item.netID);
			}
		}

		internal static void ReadID(Item item, BinaryReader reader) {
			int type = reader.ReadInt32();

			if (type >= int.MaxValue - 1) {
				bool hasCustomData = type == int.MaxValue - 1;
				string modName = reader.ReadString();
				string itemName = reader.ReadString();
				Mod mod = Loader.GetMod(modName);

				if (mod == null) {
					type = 0;
				}
				else {
					type = mod.ItemType(itemName);
				}

				item.netDefaults(type);

				if (type != 0 && hasCustomData) {
					byte[] data = reader.ReadBytes(reader.ReadInt16());

					using (MemoryStream memoryStream = new MemoryStream(data)) {
						using (BinaryReader customReader = new BinaryReader(memoryStream)) {
							item.modItem.LoadCustomData(customReader);
						}
					}
				}
			}
			else {
				item.netDefaults(type);
			}
		}

		internal static bool MeleePrefix(Item item) {
			if (item.modItem == null) {
				return false;
			}

			return item.damage > 0 && item.melee && !item.noUseGraphic;
		}

		internal static bool WeaponPrefix(Item item) {
			if (item.modItem == null) {
				return false;
			}

			return item.damage > 0 && item.melee && item.noUseGraphic;
		}

		internal static bool RangedPrefix(Item item) {
			if (item.modItem == null) {
				return false;
			}

			return item.damage > 0 && item.ranged;
		}

		internal static bool MagicPrefix(Item item) {
			if (item.modItem == null) {
				return false;
			}

			return item.damage > 0 && (item.magic || item.summon);
		}

		internal static void SetupItem(Item item) {
			if (IsModItem(item)) {
				GetItem(item.type).SetupItem(item);
			}

			foreach (GlobalItem globalItem in globalItems) {
				globalItem.SetDefaults(item);
			}
		}

		internal static void DrawAnimatedItem(Item item, Color color, Color alpha, float rotation, float scale) {
			int frameCount = Main.itemAnimations[item.type].FrameCount;
			int frameDuration = Main.itemAnimations[item.type].TicksPerFrame;
			Main.itemFrameCounter[item.whoAmI]++;

			if (Main.itemFrameCounter[item.whoAmI] >= frameDuration) {
				Main.itemFrameCounter[item.whoAmI] = 0;
				Main.itemFrame[item.whoAmI]++;
			}

			if (Main.itemFrame[item.whoAmI] >= frameCount) {
				Main.itemFrame[item.whoAmI] = 0;
			}

			Rectangle frame = ItemLoader.itemTexture[item.type].Frame(1, frameCount, 0, Main.itemFrame[item.whoAmI]);
			float offX = (item.width / 2 - frame.Width / 2);
			float offY = (item.height - frame.Height);

			Main.spriteBatch.Draw(ItemLoader.itemTexture[item.type], new Vector2(item.position.X - Main.screenPosition.X + (frame.Width / 2) + offX, item.position.Y - Main.screenPosition.Y + (frame.Height / 2) + offY), new Rectangle?(frame), alpha, rotation, frame.Size() / 2f, scale, SpriteEffects.None, 0f);

			if (item.color != default) {
				Main.spriteBatch.Draw(ItemLoader.itemTexture[item.type], new Vector2(item.position.X - Main.screenPosition.X + (frame.Width / 2) + offX, item.position.Y - Main.screenPosition.Y + (frame.Height / 2) + offY), new Rectangle?(frame), item.GetColor(color), rotation, frame.Size() / 2f, scale, SpriteEffects.None, 0f);
			}
		}

		internal static bool CanUseItem(Item item, Player player) {
			bool flag = true;

			if (IsModItem(item)) {
				flag = flag && item.modItem.CanUseItem(player);
			}

			foreach (GlobalItem globalItem in globalItems) {
				flag = flag && globalItem.CanUseItem(item, player);
			}

			return flag;
		}

		internal static void UseStyle(Item item, Player player) {
			if (IsModItem(item)) {
				item.modItem.UseStyle(player);
			}

			foreach (GlobalItem globalItem in globalItems) {
				globalItem.UseStyle(item, player);
			}
		}

		internal static void HoldStyle(Item item, Player player) {
			if (!player.pulley && player.itemAnimation <= 0) {
				if (IsModItem(item)) {
					item.modItem.HoldStyle(player);
				}

				foreach (GlobalItem globalItem in globalItems) {
					globalItem.HoldStyle(item, player);
				}
			}
		}

		internal static void HoldItem(Item item, Player player) {
			if (IsModItem(item)) {
				item.modItem.HoldItem(player);
			}

			foreach (GlobalItem globalItem in globalItems) {
				globalItem.HoldItem(item, player);
			}
		}

		internal static bool ConsumeAmmo(Item item, Item ammo, Player player) {
			if (IsModItem(item) && !item.modItem.ConsumeAmmo(player)) {
				return false;
			}

			if (IsModItem(ammo) && !ammo.modItem.ConsumeAmmo(player)) {
				return false;
			}

			foreach (GlobalItem globalItem in globalItems) {
				if (!globalItem.ConsumeAmmo(item, player) || !globalItem.ConsumeAmmo(ammo, player)) {
					return false;
				}
			}

			return true;
		}

		internal static bool Shoot(Item item, Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
			foreach (GlobalItem globalItem in globalItems) {
				if (!globalItem.Shoot(item, player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack)) {
					return false;
				}
			}

			if (IsModItem(item)) {
				if (!item.modItem.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack)) {
					return false;
				}
			}

			return true;
		}

		internal static void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox) {
			if (IsModItem(item)) {
				item.modItem.UseItemHitbox(player, ref hitbox, ref noHitbox);
			}

			foreach (GlobalItem globalItem in globalItems) {
				globalItem.UseItemHitbox(item, player, ref hitbox, ref noHitbox);
			}
		}

		internal static void MeleeEffects(Item item, Player player, Rectangle hitbox) {
			if (IsModItem(item)) {
				item.modItem.MeleeEffects(player, hitbox);
			}

			foreach (GlobalItem globalItem in globalItems) {
				globalItem.MeleeEffects(item, player, hitbox);
			}
		}

		internal static void ModifyHitNPC(Item item, Player player, NPC target, ref int damage, ref float knockBack, ref bool crit) {
			if (IsModItem(item)) {
				item.modItem.ModifyHitNPC(player, target, ref damage, ref knockBack, ref crit);
			}

			foreach (GlobalItem globalItem in globalItems) {
				globalItem.ModifyHitNPC(item, player, target, ref damage, ref knockBack, ref crit);
			}
		}

		internal static void OnHitNPC(Item item, Player player, NPC target, int damage, float knockBack, bool crit) {
			if (IsModItem(item)) {
				item.modItem.OnHitNPC(player, target, damage, knockBack, crit);
			}

			foreach (GlobalItem globalItem in globalItems) {
				globalItem.OnHitNPC(item, player, target, damage, knockBack, crit);
			}
		}

		internal static void ModifyHitPvp(Item item, Player player, Player target, ref int damage, ref bool crit) {
			if (IsModItem(item)) {
				item.modItem.ModifyHitPvp(player, target, ref damage, ref crit);
			}

			foreach (GlobalItem globalItem in globalItems) {
				globalItem.ModifyHitPvp(item, player, target, ref damage, ref crit);
			}
		}

		internal static void OnHitPvp(Item item, Player player, Player target, int damage, bool crit) {
			if (IsModItem(item)) {
				item.modItem.OnHitPvp(player, target, damage, crit);
			}

			foreach (GlobalItem globalItem in globalItems) {
				globalItem.OnHitPvp(item, player, target, damage, crit);
			}
		}

		internal static void UseItem(Item item, Player player) {
			if (IsModItem(item) && item.modItem.UseItem(player)) {
				player.itemTime = item.useTime;
			}

			foreach (GlobalItem globalItem in globalItems) {
				if (globalItem.UseItem(item, player)) {
					player.itemTime = item.useTime;
				}
			}
		}

		internal static void ConsumeItem(Item item, Player player, ref bool consume) {
			if (IsModItem(item) && !item.modItem.ConsumeItem(player)) {
				consume = false;
			}

			foreach (GlobalItem globalItem in globalItems) {
				if (!globalItem.ConsumeItem(item, player)) {
					consume = false;
				}
			}
		}

		internal static bool UseItemFrame(Item item, Player player) {
			if (IsModItem(item) && item.modItem.UseItemFrame(player)) {
				return true;
			}

			foreach (GlobalItem globalItem in globalItems) {
				if (globalItem.UseItemFrame(item, player)) {
					return true;
				}
			}

			return false;
		}

		internal static bool HoldItemFrame(Item item, Player player) {
			if (IsModItem(item) && item.modItem.HoldItemFrame(player)) {
				return true;
			}

			foreach (GlobalItem globalItem in globalItems) {
				if (globalItem.HoldItemFrame(item, player)) {
					return true;
				}
			}

			return false;
		}

		internal static void UpdateInventory(Item item, Player player) {
			if (IsModItem(item)) {
				item.modItem.UpdateInventory(player);
			}

			foreach (GlobalItem globalItem in globalItems) {
				globalItem.UpdateInventory(item, player);
			}
		}

		internal static void UpdateEquip(Item item, Player player) {
			if (IsModItem(item)) {
				item.modItem.UpdateEquip(player);
			}

			foreach (GlobalItem globalItem in globalItems) {
				globalItem.UpdateEquip(item, player);
			}
		}

		internal static void UpdateAccessory(Item item, Player player) {
			if (IsModItem(item)) {
				item.modItem.UpdateAccessory(player);
			}

			foreach (GlobalItem globalItem in globalItems) {
				globalItem.UpdateAccessory(item, player);
			}
		}

		internal static void UpdateArmorSet(Player player, Item head, Item body, Item legs) {
			if (IsModItem(head) && head.modItem.IsArmorSet(head, body, legs)) {
				head.modItem.UpdateArmorSet(player);
			}

			if (IsModItem(body) && body.modItem.IsArmorSet(head, body, legs)) {
				body.modItem.UpdateArmorSet(player);
			}

			if (IsModItem(legs) && legs.modItem.IsArmorSet(head, body, legs)) {
				legs.modItem.UpdateArmorSet(player);
			}

			foreach (GlobalItem globalItem in globalItems) {
				string set = globalItem.IsArmorSet(head, body, legs);

				if (set.Length > 0) {
					globalItem.UpdateArmorSet(player, set);
				}
			}
		}

		internal static bool CanRightClick(Item item) {
			if (IsModItem(item) && item.modItem.CanRightClick()) {
				return Main.mouseRight;
			}

			foreach (GlobalItem globalItem in globalItems) {
				if (globalItem.CanRightClick(item)) {
					return Main.mouseRight;
				}
			}

			return false;
		}

		internal static void RightClick(Item item, Player player) {
			if (Main.mouseRightRelease) {
				if (IsModItem(item)) {
					item.modItem.RightClick(player);
				}

				foreach (GlobalItem globalItem in globalItems) {
					globalItem.RightClick(item, player);
				}

				item.stack--;

				if (item.stack == 0) {
					item.SetDefaults(0, false);
				}

				SoundEngine.PlaySound(7, -1, -1, 1);

				Main.stackSplit = 30;
				Main.mouseRightRelease = false;

				Recipe.FindRecipes();
			}
		}

		internal static void DrawHair(Player player, ref bool drawHair, ref bool drawAltHair) {
			Item item = player.armor[10].headSlot >= 0 ? player.armor[10] : player.armor[0];

			if (IsModItem(item)) {
				item.modItem.DrawHair(ref drawHair, ref drawAltHair);
			}

			foreach (GlobalItem globalItem in globalItems) {
				globalItem.DrawHair(item, ref drawHair, ref drawAltHair);
			}
		}

		internal static bool DrawHead(Player player) {
			Item item = player.armor[10].headSlot >= 0 ? player.armor[10] : player.armor[0];

			if (IsModItem(item) && !item.modItem.DrawHead()) {
				return false;
			}

			foreach (GlobalItem globalItem in globalItems) {
				if (!globalItem.DrawHead(item)) {
					return false;
				}
			}

			return true;
		}

		private static Item GetWing(Player player) {
			Item item = null;

			for (int k = 3; k < 8 + player.extraAccessorySlots; k++) {
				if (player.armor[k].wingSlot > 0) {
					item = player.armor[k];
				}
			}

			return item;
		}

		internal static void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
			ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) {
			Item item = GetWing(player);

			if (item == null) {
				return;
			}

			if (IsModItem(item)) {
				item.modItem.VerticalWingSpeeds(ref ascentWhenFalling, ref ascentWhenRising, ref maxCanAscendMultiplier,
					ref maxAscentMultiplier, ref constantAscend);
			}

			foreach (GlobalItem globalItem in globalItems) {
				globalItem.VerticalWingSpeeds(item, ref ascentWhenFalling, ref ascentWhenRising,
					ref maxCanAscendMultiplier, ref maxAscentMultiplier, ref constantAscend);
			}
		}

		internal static void HorizontalWingSpeeds(Player player) {
			Item item = GetWing(player);

			if (item == null) {
				return;
			}

			if (IsModItem(item)) {
				item.modItem.HorizontalWingSpeeds(ref player.accRunSpeed, ref player.runAcceleration);
			}

			foreach (GlobalItem globalItem in globalItems) {
				globalItem.HorizontalWingSpeeds(item, ref player.accRunSpeed, ref player.runAcceleration);
			}
		}

		internal static void Update(Item item, ref float gravity, ref float maxFallSpeed) {
			if (IsModItem(item)) {
				item.modItem.Update(ref gravity, ref maxFallSpeed);
			}

			foreach (GlobalItem globalItem in globalItems) {
				globalItem.Update(item, ref gravity, ref maxFallSpeed);
			}
		}

		internal static Color? GetAlpha(Item item, Color lightColor) {
			foreach (GlobalItem globalItem in globalItems) {
				Color? color = globalItem.GetAlpha(item, lightColor);

				if (color.HasValue) {
					return color;
				}
			}

			if (IsModItem(item)) {
				return item.modItem.GetAlpha(lightColor);
			}

			return null;
		}

		internal static bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale) {
			bool flag = true;

			if (IsModItem(item) && !item.modItem.PreDrawInWorld(spriteBatch, lightColor, alphaColor, ref rotation, ref scale)) {
				flag = false;
			}

			foreach (GlobalItem globalItem in globalItems) {
				if (!globalItem.PreDrawInWorld(item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale)) {
					flag = false;
				}
			}

			return flag;
		}

		internal static void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale) {
			if (IsModItem(item)) {
				item.modItem.PostDrawInWorld(spriteBatch, lightColor, alphaColor, rotation, scale);
			}

			foreach (GlobalItem globalItem in globalItems) {
				globalItem.PostDrawInWorld(item, spriteBatch, lightColor, alphaColor, rotation, scale);
			}
		}

		internal static bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
			Color drawColor, Color itemColor, Vector2 origin, float scale) {
			bool flag = true;

			foreach (GlobalItem globalItem in globalItems) {
				if (!globalItem.PreDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale)) {
					flag = false;
				}
			}

			if (IsModItem(item) && !item.modItem.PreDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale)) {
				flag = false;
			}

			return flag;
		}

		internal static void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
			Color drawColor, Color itemColor, Vector2 origin, float scale) {
			if (IsModItem(item)) {
				item.modItem.PostDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
			}

			foreach (GlobalItem globalItem in globalItems) {
				globalItem.PostDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
			}
		}

		internal static bool CanEquipAccessory(Item item, int slot) {
			Player player = Main.player[Main.myPlayer];

			if (IsModItem(item) && !item.modItem.CanEquipAccessory(player, slot)) {
				return false;
			}

			foreach (GlobalItem globalItem in globalItems) {
				if (!globalItem.CanEquipAccessory(item, player, slot)) {
					return false;
				}
			}

			return true;
		}
	}
}