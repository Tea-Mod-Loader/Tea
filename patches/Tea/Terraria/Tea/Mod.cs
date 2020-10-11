using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria.DataStructures;

namespace Terraria.Tea
{
	public abstract class Mod
	{
		internal string filePath;
		internal Assembly ModAssembly;
        internal readonly List<ModRecipe> recipes = new List<ModRecipe>();
		internal readonly IDictionary<string, ModItem> items = new Dictionary<string, ModItem>();
		internal readonly IDictionary<string, GlobalItem> globalItems = new Dictionary<string, GlobalItem>();
		internal readonly IDictionary<string, ModNPC> npcs = new Dictionary<string, ModNPC>();
		internal readonly IDictionary<string, GlobalNPC> globalNPCs = new Dictionary<string, GlobalNPC>();

		private string name;

		public string Name { get { return name; } }

		internal void Initialize() {
			SetModInfo(out name);
		}

		public abstract void SetModInfo(out string name);

		public virtual void Load() { }

		public virtual void AddRecipes() { }

		internal void SetupContent() {
			foreach (ModItem item in items.Values) {
				ItemLoader.itemTexture[item.item.type] = Loader.GetTexture(item.mod.ModAssembly.GetName().Name + item.texture);
				item.SetDefaults();
				DrawAnimation animation = item.GetAnimation();

				if (animation != null) {
					Main.RegisterItemAnimation(item.item.type, animation);
					ItemLoader.animations.Add(item.item.type);
				}
			}
			foreach (ModNPC npc in npcs.Values) {
				NPCLoader.npcTexture[npc.npc.type] = Loader.GetTexture(npc.mod.ModAssembly.GetName().Name + npc.texture);
				npc.SetDefaults();
				if (npc.npc.lifeMax > 32767 || npc.npc.boss) {
					Main.npcLifeBytes[npc.npc.type] = 4;
				}
				else if (npc.npc.lifeMax > 127) {
					Main.npcLifeBytes[npc.npc.type] = 2;
				}
				else {
					Main.npcLifeBytes[npc.npc.type] = 1;
				}
			}
		}

		internal void Autoload() {
			Type[] classes = ModAssembly.GetTypes();

			foreach (Type type in classes) {
				if (type.IsSubclassOf(typeof(ModItem))) {
					AutoloadItem(type);
				}

				if (type.IsSubclassOf(typeof(GlobalItem))) {
					AutoloadGlobalItem(type);
				}
				if (type.IsSubclassOf(typeof(ModNPC))) {
					AutoloadNPC(type);
				}
				if (type.IsSubclassOf(typeof(GlobalNPC))) {
					AutoloadGlobalNPC(type);
				}
			}
		}

		internal void Unload() {
			recipes.Clear();
			items.Clear();
			globalItems.Clear();
			npcs.Clear();
			globalNPCs.Clear();
		}

		public void AddItem(string name, ModItem item, string texture) {
			int id = ItemLoader.ReserveItemID();
			item.item.SetNameOverride(name);
			item.item.ResetStats(id);
			items[name] = item;
			ItemLoader.items[id] = item;
			item.texture = texture;
			item.mod = this;
		}

		public void AddGlobalItem(string name, GlobalItem globalItem) {
			globalItem.mod = this;
			globalItem.Name = name;
			globalItems[name] = globalItem;

			ItemLoader.globalItems.Add(globalItem);
		}

		public void AddNPC(string name, ModNPC npc, string texture) {
			int id = NPCLoader.ReserveNPCID();
			npc.npc.name = name;
			npc.npc.type = id;
			npcs[name] = npc;
			NPCLoader.npcs[id] = npc;
			npc.texture = texture;
			npc.mod = this;
		}

		public void AddGlobalNPC(string name, GlobalNPC globalNPC) {
			globalNPC.mod = this;
			globalNPC.Name = name;
			this.globalNPCs[name] = globalNPC;
			NPCLoader.globalNPCs.Add(globalNPC);
		}


		// TODO: AddEquipTexture
		/*public int AddEquipTexture(ModItem item, EquipType type, string texture, string armTexture = "", string femaleTexture = "") {
			int slot = EquipLoader.ReserveEquipID(type);
			EquipLoader.equips[type][texture] = slot;

			ModLoader.GetTexture(texture);

			if (type == EquipType.Body) {
				EquipLoader.armTextures[slot] = armTexture;
				EquipLoader.femaleTextures[slot] = femaleTexture.Length > 0 ? femaleTexture : texture;

				ModLoader.GetTexture(armTexture);
				ModLoader.GetTexture(femaleTexture);
			}

			if (type == EquipType.Head || type == EquipType.Body || type == EquipType.Legs) {
				EquipLoader.slotToId[type][slot] = item.item.type;
			}

			return slot;
		}*/

		private void AutoloadItem(Type type) {
			ModItem item = (ModItem)Activator.CreateInstance(type);

			item.mod = this;
			string name = type.Name;
			string texture = (type.Namespace + "." + type.Name).Replace('.', '/');
			// TODO: IList<EquipType> equips = new List<EquipType>();

			if (item.Autoload(ref name, ref texture/*, equips*/)) {

				AddItem(name, item, texture);

				/*if (equips.Count > 0) {
					EquipLoader.idToSlot[item.item.type] = new Dictionary<EquipType, int>();

					foreach (EquipType equip in equips) {
						string equipTexture = texture + "_" + equip.ToString();
						string armTexture = texture + "_Arms";
						string femaleTexture = texture + "_FemaleBody";

						item.AutoloadEquip(equip, ref equipTexture, ref armTexture, ref femaleTexture);
						int slot = AddEquipTexture(item, equip, equipTexture, armTexture, femaleTexture);
						EquipLoader.idToSlot[item.item.type][equip] = slot;
					}
				}*/
			}
		}

		private void AutoloadGlobalItem(Type type) {
			GlobalItem globalItem = (GlobalItem)Activator.CreateInstance(type);

			globalItem.mod = this;
			string name = type.Name;

			if (globalItem.Autoload(ref name)) {
				AddGlobalItem(name, globalItem);
			}
		}

		private void AutoloadNPC(Type type) {
			ModNPC npc = (ModNPC)Activator.CreateInstance(type);
			npc.mod = this;
			string name = type.Name;
			string texture = (type.Namespace + "." + type.Name).Replace('.', '/');
			if (npc.Autoload(ref name, ref texture)) {
				AddNPC(name, npc, texture);
			}
		}

		private void AutoloadGlobalNPC(Type type) {
			GlobalNPC globalNPC = (GlobalNPC)Activator.CreateInstance(type);
			globalNPC.mod = this;
			string name = type.Name;
			if (globalNPC.Autoload(ref name)) {
				AddGlobalNPC(name, globalNPC);
			}
		}


		public ModItem GetItem(string name) {
			if (items.ContainsKey(name)) {
				return items[name];
			}
			else {
				return null;
			}
		}

		public ModNPC GetNPC(string name) {
			if (npcs.ContainsKey(name)) {
				return npcs[name];
			}
			else {
				return null;
			}
		}

		public GlobalNPC GetGlobalNPC(string name) {
			if (this.globalNPCs.ContainsKey(name)) {
				return this.globalNPCs[name];
			}
			else {
				return null;
			}
		}

		public GlobalItem GetGlobalItem(string name) {
			if (globalItems.ContainsKey(name)) {
				return globalItems[name];
			}
			else {
				return null;
			}
		}


		public int ItemType(string name) {
			ModItem item = GetItem(name);

			if (item == null) {
				return 0;
			}

			return item.item.type;
		}

		public int NPCType(string name) {
			ModNPC npc = GetNPC(name);
			if (npc == null) {
				return 0;
			}
			return npc.npc.type;
		}
	}
}
