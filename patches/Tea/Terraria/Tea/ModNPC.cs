using System;

namespace Terraria.Tea
{
	public class ModNPC
	{
		public NPC npc { get; internal set; }

		public Mod mod { get; internal set; }

		internal string texture;

		public ModNPC() => npc = new NPC();

		public virtual bool Autoload(ref string name, ref string texture) => true;

		internal void SetupNPC(NPC npc) {
			ModNPC newNPC = (ModNPC)Activator.CreateInstance(GetType());
			newNPC.npc = npc;
			npc.modNPC = newNPC;
			newNPC.mod = mod;
			newNPC.SetDefaults();
		}

		public virtual void SetDefaults() { }

		public virtual float CanSpawn(NPCSpawnInfo spawnInfo) => 0f;

		public virtual int SpawnNPC(int tileX, int tileY) => NPC.NewNPC(tileX * 16 + 8, tileY * 16, npc.type);
	}
}
