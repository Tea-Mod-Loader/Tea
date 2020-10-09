using System.Reflection;

namespace Terraria.Tea
{
	public abstract class Mod
	{
		internal string filePath;
		internal Assembly ModAssembly;

		private string name;

		public string Name { get { return name; } }

		internal void Initialize() {
			SetModInfo(out name);
		}

		public abstract void SetModInfo(out string name);

		public virtual void Load() { }

		public virtual void Unload() { }

		public virtual void AddRecipes() { }

		internal void Autoload() { }

		// public void AddItem(string name, ModItem item, string texture) { }

		internal void SetupContent() { }
	}
}
