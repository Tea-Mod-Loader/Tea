using Microsoft.CSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Content.Readers;
using ReLogic.Graphics;
using ReLogic.Utilities;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Terraria.ModLoader
{
	/// <summary>
	/// The class in charge of loading mods.
	/// </summary>
	public static class ModLoader {
		private static readonly IDictionary<string, Mod> _mods = new Dictionary<string, Mod>();
		private static readonly IList<string> _buildReferences = new List<string>();
		private static bool _referencesLoaded = false;

		internal static string _modToBuild;

		public static readonly string ModPath = Main.SavePath + Path.DirectorySeparatorChar + "Mods";
		public static readonly string ModSourcePath = Main.SavePath + Path.DirectorySeparatorChar + "Mod Sources";
		public static readonly string DllPath = Main.SavePath + Path.DirectorySeparatorChar + "dllReferences";
		public static AssetReaderCollection ModAssetReaderCollection;
		public static AsyncAssetLoader ModAsyncAssetLoader;
		public static IAssetRepository ModAssetRepository;

		/// <summary>
		/// Grabs the specified mod from <c>_mods</c>. If the mod isn't guaranteed to be loaded alongside your own, use <c>TryGetMod(string name, out Mod mod)</c>.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static Mod GetMod(string name) => _mods[name];

		/// <summary>
		/// Attemps to grab the specified mod from the <c>_mods</c> dictionary. If the mod isn't found, it's <c>null</c>.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="mod"></param>
		public static void TryGetMod(string name, out Mod mod) {
			if (_mods.ContainsKey(name))
				mod = _mods[name];
			else
				mod = null;
		}

		/// <summary>
		/// Returns an array of strings that contains every mod name is the user's Mods folder.
		/// </summary>
		/// <returns></returns>
		public static string[] FindMods() {
			Directory.CreateDirectory(ModPath);

			return Directory.GetFiles(ModPath, "*.tea", SearchOption.TopDirectoryOnly);
		}

		public static Asset<Texture2D> GetTexture(string name) => ModAssetRepository.Request<Texture2D>(name);

		#region Internal/Private Stuff
		internal static void Load() => ThreadPool.QueueUserWorkItem(new WaitCallback(delegate {
			LoadMods();

			int currentMod = 0;

			foreach (Mod mod in _mods.Values) {
				try {
					mod.AutoloadInternal();
					mod.Load();
				}
				catch (Exception e) {
					Logging.LogClient(e.Message + "\n" + e.StackTrace);
					return;
				}

				currentMod++;
			}

			ResizeArrays();

			currentMod = 0;
			foreach (Mod mod in _mods.Values) {
				try {
					mod.SetupContent();
				}
				catch (Exception e) {
					Logging.LogClient(e.Message + "\n" + e.StackTrace);
				}
			}
		}), 1);

		private static void ResizeArrays(bool unloading = false) {
			// TODO
		}

		private static void LoadMods() {
			string[] modFiles = FindMods();
			List<string> modsToLoad = new List<string>();

			foreach (string modFile in modFiles)
				modsToLoad.Add(modFile);

			int previousCount = 0;
			int modCount = 0;

			while (modsToLoad.Count > 0 && modsToLoad.Count != previousCount) {
				previousCount = modsToLoad.Count;

				for (int i = 0; i < modsToLoad.Count; i++) {
					try {
						LoadMod(modsToLoad[i]);
					}
					catch(Exception e) {
						Logging.LogClient(e.Message + "\n" + e.StackTrace);
					}

					modCount++;

					modsToLoad.RemoveAt(i);
				}
			}
		}

		private static void LoadMod(string modFile) {
			Directory.CreateDirectory(DllPath);

			Assembly modCode;

			using (FileStream fileStream = File.OpenRead(modFile)) {
				using (BinaryReader reader = new BinaryReader(fileStream)) {
					fileStream.Seek(reader.ReadInt32(), SeekOrigin.Current);

					modCode = Assembly.Load(reader.ReadBytes(reader.ReadInt32()));

					for (string texturePath = reader.ReadString(); texturePath != "end"; texturePath = reader.ReadString()) {
						ModAssetRepository.Request<Texture2D>(texturePath);
					}
				}
			}

			Type[] classes = modCode.GetTypes();

			foreach (Type type in classes) {
				if (type.IsSubclassOf(typeof(Mod))) {
					Mod mod = (Mod)Activator.CreateInstance(type);
					mod.file = modFile;
					mod.code = modCode;
					mod.Initialize();
					_mods[mod.InternalName] = mod;
				}
			}
		}

		internal static void InitializeModAssetLoader(GameServiceContainer services) {
			ModAssetReaderCollection = new AssetReaderCollection();
			ModAssetReaderCollection.RegisterReader(new PngReader(services.Get<IGraphicsDeviceService>().GraphicsDevice), ".png");
			ModAsyncAssetLoader = new AsyncAssetLoader(ModAssetReaderCollection);
			ModAsyncAssetLoader.RequireTypeCreationOnTransfer(typeof(Texture2D));
			ModAsyncAssetLoader.RequireTypeCreationOnTransfer(typeof(DynamicSpriteFont));
			ModAsyncAssetLoader.RequireTypeCreationOnTransfer(typeof(SpriteFont));
			ModAssetRepository = new AssetRepository(new AssetLoader(ModAssetReaderCollection), ModAsyncAssetLoader);
		}

		private static string[] FindModSources() {
			Directory.CreateDirectory(ModSourcePath);

			return Directory.GetDirectories(ModSourcePath, "*", SearchOption.TopDirectoryOnly).Where(x => x != ".vs").ToArray();
		}

		internal static void BuildAllMods() => ThreadPool.QueueUserWorkItem(new WaitCallback(delegate(object threadContext) {
			string[] modFolders = FindModSources();
			int modCount = 0;

			foreach (string modFolder in modFolders) {
				_modToBuild = modFolder;
				do_BuildMod(threadContext);
				modCount++;
			}
		}), 1);

		internal static void BuildMod() => ThreadPool.QueueUserWorkItem(new WaitCallback(delegate(object threadContext) { do_BuildMod(threadContext); }), 1);

		private static bool do_BuildMod(object threadContext) {
			LoadReferences();

			if (!CompileMod(_modToBuild)) {
				return false;
			}

			string file = ModPath + Path.DirectorySeparatorChar + Path.GetFileName(_modToBuild) + ".tea";
			byte[] buffer = File.ReadAllBytes(file);

			using (FileStream fileStream = File.Create(file)) {
				using (BinaryWriter writer = new BinaryWriter(fileStream)) {
					writer.Write(buffer.Length);
					writer.Write(buffer);

					string[] images = Directory.GetFiles(_modToBuild, "*.png", SearchOption.AllDirectories);

					foreach (string image in images) {
						string texturePath = image.Replace(ModSourcePath + Path.DirectorySeparatorChar, null);
						texturePath = Path.ChangeExtension(texturePath.Replace(Path.DirectorySeparatorChar, '/'), null);
						buffer = File.ReadAllBytes(image);
						writer.Write(texturePath);
						writer.Write(buffer.Length);
						writer.Write(buffer);
					}

					writer.Write("end");
				}
			}

			return true;
		}

		private static void LoadReferences() {
			if (_referencesLoaded) {
				return;
			}

			Assembly assembly = Assembly.GetExecutingAssembly();
			_buildReferences.Add(assembly.Location);
			AssemblyName[] references = assembly.GetReferencedAssemblies();

			foreach (AssemblyName reference in references) {
				_buildReferences.Add(Assembly.Load(reference).Location);
			}

			_referencesLoaded = true;
		}

		private static bool CompileMod(string modDir) {
			CompilerParameters compileOptions = new CompilerParameters();
			compileOptions.GenerateExecutable = false;
			compileOptions.GenerateInMemory = false;
			compileOptions.OutputAssembly = ModPath + Path.DirectorySeparatorChar + Path.GetFileName(modDir) + ".tea";

			foreach (string reference in _buildReferences) {
				compileOptions.ReferencedAssemblies.Add(reference);
			}

			Directory.CreateDirectory(DllPath);

			CodeDomProvider codeProvider = new CSharpCodeProvider();
			CompilerResults results = codeProvider.CompileAssemblyFromFile(compileOptions, Directory.GetFiles(modDir, "*.cs", SearchOption.AllDirectories));
			CompilerErrorCollection errors = results.Errors;

			if (errors.HasErrors) {
				foreach (CompilerError error in errors) {
					Logging.LogClient(error.ToString());
				}
				return false;
			}

			return true;
		}
		#endregion
	}
}
