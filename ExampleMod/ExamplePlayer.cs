using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod
{
	public class ExamplePlayer : ModPlayer
	{
		private const int saveVersion = 0;
		public int score = 0;
		public bool elementShield = false;
		private int elementShields = 0;
		public int voidMonolith = 0;
		public int constantDamage = 0;
		public float percentDamage = 0f;

		public override void ResetEffects()
		{
			elementShield = false;
			if (voidMonolith > 0)
			{
				voidMonolith--; //this is a very bad hack until I create ModWorld
			}
		}

		public override void SaveCustomData(BinaryWriter writer)
		{
			writer.Write(saveVersion);
			writer.Write(score);
		}

		public override void LoadCustomData(BinaryReader reader)
		{
			int loadVersion = reader.ReadInt32();
			score = reader.ReadInt32();
		}

		public override void SetupStartInventory(IList<Item> items)
		{
			Item item = new Item();
			item.SetDefaults(mod.ItemType("ExampleItem"));
			item.stack = 5;
			items.Add(item);
		}

		public override void UpdateBiomeVisuals()
		{
			bool usePurity = NPC.AnyNPCs(mod.NPCType("PuritySpirit"));
			player.ManageSpecialBiomeVisuals("ExampleMod:PuritySpirit", usePurity);
			bool useVoidMonolith = voidMonolith > 0 && !usePurity && !NPC.AnyNPCs(NPCID.MoonLordCore);
			player.ManageSpecialBiomeVisuals("ExampleMod:MonolithVoid", useVoidMonolith, player.Center);
		}

		public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit,
			ref bool customDamage, ref bool playSound, ref bool genGore, ref string deathText)
		{
			if (constantDamage > 0 || percentDamage > 0f)
			{
				int damageFromPercent = (int)(player.statLifeMax2 * percentDamage);
				damage = Math.Max(constantDamage, damageFromPercent);
				customDamage = true;
			}
			constantDamage = 0;
			percentDamage = 0f;
			return base.PreHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage,
				ref playSound, ref genGore, ref deathText);
		}

		public override void AnglerQuestReward(float quality, List<Item> rewardItems)
		{
			if (voidMonolith > 0)
			{
				Item sticky = new Item();
				sticky.SetDefaults(ItemID.StickyDynamite);
				sticky.stack = 4;
				rewardItems.Add(sticky);
			}
			foreach (Item item in rewardItems)
			{
				if (item.type == ItemID.GoldCoin)
				{
					int stack = item.stack;
					item.SetDefaults(ItemID.PlatinumCoin);
					item.stack = stack;
				}
			}
		}

		public override void CatchFish(Item fishingRod, Item bait, int liquidType, int poolSize, int worldLayer, int questFish, ref int caughtType, ref bool junk)
		{
			if (junk)
			{
				return;
			}
			if (player.HasBuff(BuffID.TwinEyesMinion) > -1 && liquidType == 0 && Main.rand.Next(3) == 0)
			{
				caughtType = mod.ItemType("SparklingSphere");
			}
			if (player.gravDir == -1f && questFish == mod.ItemType("ExampleQuestFish") && Main.rand.Next(2) == 0)
			{
				caughtType = mod.ItemType("ExampleQuestFish");
			}
		}

		public override void GetFishingLevel(Item fishingRod, Item bait, ref int fishingLevel)
		{
			if (player.HasBuff(mod.BuffType("CarMount")) > -1)
			{
				fishingLevel = (int)(fishingLevel * 1.1f);
			}
		}

		public override void GetDyeTraderReward(List<int> dyeItemIDsPool)
		{
			if (player.HasBuff(BuffID.UFOMount) > -1)
			{
				dyeItemIDsPool.Clear();
				dyeItemIDsPool.Add(ItemID.MartianArmorDye);
			}
		}
	}
}