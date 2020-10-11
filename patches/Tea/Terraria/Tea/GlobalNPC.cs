using System.Collections.Generic;

namespace Terraria.Tea
{
	public class GlobalNPC
	{
		public Mod mod { get; internal set; }

		public string Name { get; internal set; }

		public virtual bool Autoload(ref string name) => true;

		public virtual void SetDefaults(NPC npc) { }

		public virtual void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns) { }

		public virtual void EditSpawnRange(Player player, ref int spawnRangeX, ref int spawnRangeY,
			ref int safeRangeX, ref int safeRangeY) { }

		public virtual void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo) { }

		public virtual void SpawnNPC(int npc, int tileX, int tileY) { }

		// TODO: NPCLoot
	}
}
