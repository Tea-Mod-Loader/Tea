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
			}
		}

		internal void Unload() {
			recipes.Clear();
			items.Clear();
			globalItems.Clear();
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


		public ModItem GetItem(string name) {
			if (items.ContainsKey(name)) {
				return items[name];
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

		public GlobalItem GetGlobalItem(string name) {
			if (globalItems.ContainsKey(name)) {
				return globalItems[name];
			}
			else {
				return null;
			}
		}
	}
}
