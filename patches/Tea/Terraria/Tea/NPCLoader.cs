using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.Localization;

namespace Terraria.Tea
{
	public static class NPCLoader
	{
		private static int nextNPC = NPCID.Count;

		internal static readonly IDictionary<int, ModNPC> npcs = new Dictionary<int, ModNPC>();
		internal static readonly IList<GlobalNPC> globalNPCs = new List<GlobalNPC>();

		public static int Count = nextNPC;
        public static Texture2D[] npcTexture = new Texture2D[NPCID.Count];

		internal static int ReserveNPCID() {
			int reserveID = nextNPC;
			nextNPC++;

			return reserveID;
		}

		public static ModNPC GetNPC(int type) {
			if (npcs.ContainsKey(type)) {
				return npcs[type];
			}
			else {
				return null;
			}
		}

		internal static void ResizeArrays() {
            Array.Resize(ref npcTexture, nextNPC);
			Array.Resize(ref TextureAssets.Npc, nextNPC);
			//LoaderUtils.ResetStaticMembers(typeof(NPCID), true);
			Array.Resize(ref Main.townNPCCanSpawn, nextNPC);
			Array.Resize(ref Main.slimeRainNPC, nextNPC);
			Array.Resize(ref Main.npcCatchable, nextNPC);
			Array.Resize(ref Main.npcFrameCount, nextNPC);
			Array.Resize(ref NPC.killCount, nextNPC);
			Array.Resize(ref NPC.npcsFoundForCheckActive, nextNPC);
			Array.Resize(ref Lang._npcNameCache, nextNPC);
			Array.Resize(ref EmoteBubble.CountNPCs, nextNPC);
			Array.Resize(ref WorldGen.TownManager._hasRoom, nextNPC);
            Array.Resize(ref NPCID.Sets.DontDoHardmodeScaling, nextNPC);
            Array.Resize(ref NPCID.Sets.IsTownPet, nextNPC);
            Array.Resize(ref NPCID.Sets.CantTakeLunchMoney, nextNPC);
            Array.Resize(ref NPCID.Sets.TrailingMode, nextNPC);
            Array.Resize(ref NPCID.Sets.IsDragonfly, nextNPC);
            Array.Resize(ref NPCID.Sets.BelongsToInvasionOldOnesArmy, nextNPC);
            Array.Resize(ref NPCID.Sets.TeleportationImmune, nextNPC);
            Array.Resize(ref NPCID.Sets.UsesNewTargetting, nextNPC);
            Array.Resize(ref NPCID.Sets.TakesDamageFromHostilesWithoutBeingFriendly, nextNPC);
            Array.Resize(ref NPCID.Sets.AllNPCs, nextNPC);
            Array.Resize(ref NPCID.Sets.HurtingBees, nextNPC);
            Array.Resize(ref NPCID.Sets.FighterUsesDD2PortalAppearEffect, nextNPC);
            Array.Resize(ref NPCID.Sets.StatueSpawnedDropRarity, nextNPC);
            Array.Resize(ref NPCID.Sets.NoEarlymodeLootWhenSpawnedFromStatue, nextNPC);
            Array.Resize(ref NPCID.Sets.NeedsExpertScaling, nextNPC);
            Array.Resize(ref NPCID.Sets.ProjectileNPC, nextNPC);
            Array.Resize(ref NPCID.Sets.SavesAndLoads, nextNPC);
            Array.Resize(ref NPCID.Sets.TrailCacheLength, nextNPC);
            Array.Resize(ref NPCID.Sets.NoMultiplayerSmoothingByType, nextNPC);
            Array.Resize(ref NPCID.Sets.NoMultiplayerSmoothingByAI, nextNPC);
            Array.Resize(ref NPCID.Sets.MPAllowedEnemies, nextNPC);
            Array.Resize(ref NPCID.Sets.TownCritter, nextNPC);
            Array.Resize(ref NPCID.Sets.CountsAsCritter, nextNPC);
            Array.Resize(ref NPCID.Sets.HasNoPartyText, nextNPC);
            Array.Resize(ref NPCID.Sets.HatOffsetY, nextNPC);
            Array.Resize(ref NPCID.Sets.FaceEmote, nextNPC);
            Array.Resize(ref NPCID.Sets.ExtraFramesCount, nextNPC);
            Array.Resize(ref NPCID.Sets.AttackFrameCount, nextNPC);
            Array.Resize(ref NPCID.Sets.DangerDetectRange, nextNPC);
            Array.Resize(ref NPCID.Sets.AttackTime, nextNPC);
            Array.Resize(ref NPCID.Sets.AttackAverageChance, nextNPC);
            Array.Resize(ref NPCID.Sets.AttackType, nextNPC);
            Array.Resize(ref NPCID.Sets.PrettySafe, nextNPC);
            Array.Resize(ref NPCID.Sets.MagicAuraColor, nextNPC);
            Array.Resize(ref NPCID.Sets.DemonEyes, nextNPC);
            Array.Resize(ref NPCID.Sets.Zombies, nextNPC);
            Array.Resize(ref NPCID.Sets.Skeletons, nextNPC);
            Array.Resize(ref NPCID.Sets.BossHeadTextures, nextNPC);
            Array.Resize(ref NPCID.Sets.PositiveNPCTypesExcludedFromDeathTally, nextNPC);
            Array.Resize(ref NPCID.Sets.ShouldBeCountedAsBoss, nextNPC);
            Array.Resize(ref NPCID.Sets.DangerThatPreventsOtherDangers, nextNPC);
            Array.Resize(ref NPCID.Sets.MustAlwaysDraw, nextNPC);
            Array.Resize(ref NPCID.Sets.ExtraTextureCount, nextNPC);
            Array.Resize(ref NPCID.Sets.NPCFramingGroup, nextNPC);

			for (int k = NPCID.Count; k < nextNPC; k++) {
				Main.npcFrameCount[k] = 1;
				Lang._npcNameCache[k] = LocalizedText.Empty;
			}
		}

		internal static void Unload() {
			npcs.Clear();
			nextNPC = NPCID.Count;
			globalNPCs.Clear();
		}

		public static bool IsModNPC(NPC npc) => npc.type >= NPCID.Count;

		public static void SetupNPC(NPC npc) {
			if (IsModNPC(npc)) {
				GetNPC(npc.type).SetupNPC(npc);
			}

			foreach (GlobalNPC globalNPC in globalNPCs) {
				globalNPC.SetDefaults(npc);
			}
		}

		internal static int ChooseSpawn(NPCSpawnInfo spawnInfo) {
			IDictionary<int, float> pool = new Dictionary<int, float>();
			pool[0] = 1f;
			foreach (ModNPC npc in npcs.Values) {
				float weight = npc.CanSpawn(spawnInfo);
				if (weight > 0f) {
					pool[npc.npc.type] = weight;
				}
			}
			foreach (GlobalNPC globalNPC in globalNPCs) {
				globalNPC.EditSpawnPool(pool, spawnInfo);
			}
			float totalWeight = 0f;
			foreach (int type in pool.Keys) {
				totalWeight += pool[type];
			}
			float choice = (float)Main.rand.NextDouble() * totalWeight;
			foreach (int type in pool.Keys) {
				float weight = pool[type];
				if (choice < weight) {
					return type;
				}
				choice -= weight;
			}
			return 0;
		}

		internal static int SpawnNPC(int type, int tileX, int tileY) {
			int npc;
			if (type >= NPCID.Count) {
				npc = NPCLoader.npcs[type].SpawnNPC(tileX, tileY);
			}
			else {
				npc = NPC.NewNPC(tileX * 16 + 8, tileY * 16, type);
			}
			foreach (GlobalNPC globalNPC in globalNPCs) {
				globalNPC.SpawnNPC(npc, tileX, tileY);
			}
			return npc;
		}

		internal static void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns) {
			foreach (GlobalNPC globalNPC in globalNPCs) {
				globalNPC.EditSpawnRate(player, ref spawnRate, ref maxSpawns);
			}
		}

		internal static void EditSpawnRange(Player player, ref int spawnRangeX, ref int spawnRangeY,
			ref int safeRangeX, ref int safeRangeY) {
			foreach (GlobalNPC globalNPC in globalNPCs) {
				globalNPC.EditSpawnRange(player, ref spawnRangeX, ref spawnRangeY, ref safeRangeX, ref safeRangeY);
			}
		}
	}
}
