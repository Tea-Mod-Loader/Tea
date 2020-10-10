﻿using Microsoft.CSharp;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Terraria.ID;

namespace Terraria.Tea
{
	public class Loader {
		public static readonly Version teaVersion = new Version(0, 0, 1, 0);
		public static readonly string ModSavePath = Main.SavePath + Path.DirectorySeparatorChar + "Mods";
		public static readonly string ReferencePath = Main.SavePath + Path.DirectorySeparatorChar + "dllReferences";
		public static readonly string ModSourcesPath = Main.SavePath + Path.DirectorySeparatorChar + "Mod Sources";
        public static readonly string branchName = "master";
		public static readonly int beta = 1;
		public static readonly string versionedName = $"Tea v{teaVersion}" +
											  (branchName.Length == 0 ? "" : $" {branchName}") +
											  (beta == 0 ? "" : $" Beta {beta}");

		public static readonly string versionTag = $"v{teaVersion}" +
													(branchName.Length == 0 ? "" : $"-{branchName.ToLower()}") +
													(beta == 0 ? "" : $"-beta{beta}");


		internal static bool buildAll = false;
		internal static bool reloadAfterBuild = false;
		internal static string modToBuild;
		internal static readonly IDictionary<string, Mod> mods = new Dictionary<string, Mod>();
		internal const bool IsBeta = true;

		private static bool loadedAllReferences = false;
		private static readonly IList<string> buildReferences = new List<string>();
		private static readonly IList<string> loadedMods = new List<string>();
		private static readonly IDictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

		private static void LoadReferences() {
			if (!loadedAllReferences) {
				Assembly executingAssembly = Assembly.GetExecutingAssembly();
				buildReferences.Add(executingAssembly.Location);

				foreach (AssemblyName reference in executingAssembly.GetReferencedAssemblies()) {
					buildReferences.Add(Assembly.Load(reference).Location);
				}

				loadedAllReferences = true;
			}
		}

		internal static bool ModLoaded(string name) => loadedMods.Contains(name);

		public static Mod GetMod(string name) {
			if (mods.ContainsKey(name)) {
				return mods[name];
			}

			return null;
		}

		internal static void Load() => ThreadPool.QueueUserWorkItem(new WaitCallback(do_Load), 1);

		public static void do_Load(object threadContext) {
			if (!LoadMods()) {
				Main.menuMode = LoaderMenus.errorMessageID;

				return;
			}

			int modsLoaded = 0;

			foreach (Mod mod in mods.Values) {
				LoaderMenus.loadMods.SetProgressInit(mod.Name, modsLoaded, mods.Count);

				try {
					mod.Autoload();
					mod.Load();
				}
				catch (Exception e) {
					DisableMod(mod.filePath);
					ErrorLogger.LogLoadingError(mod.filePath, e);

					Main.menuMode = LoaderMenus.errorMessageID;

					return;
				}

				modsLoaded++;
			}

			LoaderMenus.loadMods.SetProgressSetup(0f);
			ResizeArrays();

			modsLoaded = 0;

			foreach (Mod mod in mods.Values) {
				LoaderMenus.loadMods.SetProgressLoad(mod.Name, modsLoaded, mods.Count);

				try {
					mod.SetupContent();
				}
				catch (Exception e) {
					DisableMod(mod.filePath);
                    ErrorLogger.LogLoadingError(mod.filePath, e);

					Main.menuMode = LoaderMenus.errorMessageID;
				}

				modsLoaded = 0;
			}

			LoaderMenus.loadMods.SetProgressRecipes();

			Recipe.numRecipes = 0;

			try {
				Recipe.SetupRecipes();
			}
			catch (Exception e) {
                ErrorLogger.LogLoadingError("recipes", e);

				Main.menuMode = LoaderMenus.errorMessageID;

				return;
			}

			Main.menuMode = 0;
		}

		private static void ResizeArrays(bool unloading = false) {
			ItemLoader.ResizeArrays();
		}

		internal static string[] FindMods() {
			Directory.CreateDirectory(ModSavePath);

			return Directory.GetFiles(ModSavePath, "*.tea", SearchOption.TopDirectoryOnly);
		}

		private static bool LoadMods() {
			LoaderMenus.loadMods.SetProgressFinding();

			string[] modFiles = FindMods();
			List<string> enabledMods = new List<string>();

			foreach (string modFile in modFiles) {
				if (IsEnabled(modFile)) {
					enabledMods.Add(modFile);
				}
			}

			IDictionary<string, BuildProperties> properties = new Dictionary<string, BuildProperties>();
			List<string> modsToLoad = new List<string>();

			foreach (string modFile in enabledMods) {
                properties[modFile] = LoadBuildProperties(modFile);
				modsToLoad.Add(modFile);
			}

			int previousCount = 0;
			int modsCapableOfBeingLoaded = 0;

			while (modsToLoad.Count > 0 && modsToLoad.Count != previousCount) {
				previousCount = modsToLoad.Count;
				int k = 0;

				while (k < modsToLoad.Count) {
					bool canLoad = true;

					foreach (string reference in properties[modsToLoad[k]].modReferences) {
						if (!ModLoaded(ModSavePath + Path.DirectorySeparatorChar + reference + ".tea")) {
							canLoad = false;

							break;
						}
					}

					if (canLoad) {
						LoaderMenus.loadMods.SetProgressReading(Path.GetFileNameWithoutExtension(modsToLoad[k]), modsCapableOfBeingLoaded, enabledMods.Count);

						try {
							LoadMod(modsToLoad[k], properties[modsToLoad[k]]);
						}
						catch (Exception e) {
							DisableMod(modsToLoad[k]);
                            ErrorLogger.LogLoadingError(modsToLoad[k], e);

							return false;
						}

						loadedMods.Add(modsToLoad[k]);

						modsCapableOfBeingLoaded++;
						modsToLoad.RemoveAt(k);
					}
					else {
						k++;
					}
				}
			}

			if (modsToLoad.Count > 0) {
				foreach (string modFile in modsToLoad) {
					DisableMod(modFile);
				}
                ErrorLogger.LogMissingLoadReference(modsToLoad);

				return false;
			}

			return true;

		}


		private static void LoadMod(string modFile, BuildProperties properties) {
			Directory.CreateDirectory(ReferencePath);

			foreach (string dllReference in properties.dllReferences) {
				string dllFile = ReferencePath + Path.DirectorySeparatorChar + dllReference + ".dll";

				Assembly.Load(File.ReadAllBytes(dllFile));
			}

			Assembly modCode;

			using (FileStream fileStream = File.OpenRead(modFile)) {
				using (BinaryReader reader = new BinaryReader(fileStream)) {
					fileStream.Seek(reader.ReadInt32(), SeekOrigin.Current);
					modCode = Assembly.Load(reader.ReadBytes(reader.ReadInt32()));
					for (string texturePath = reader.ReadString(); texturePath != "end"; texturePath = reader.ReadString()) {
						byte[] imageData = reader.ReadBytes(reader.ReadInt32());
						using (MemoryStream buffer = new MemoryStream(imageData)) {
							textures[texturePath] = Texture2D.FromStream(Main.instance.GraphicsDevice, buffer);
						}
					}
				}
			}

			Type[] classes = modCode.GetTypes();

			foreach (Type type in classes) {
				if (type.IsSubclassOf(typeof(Mod))) {
					Mod mod = (Mod)Activator.CreateInstance(type);
					mod.filePath = modFile;
					mod.ModAssembly = modCode;
					mod.Initialize();
					mods[mod.Name] = mod;
				}
			}
		}

		internal static BuildProperties LoadBuildProperties(string modFile) {
			BuildProperties properties = new BuildProperties();
			byte[] data;

			using (FileStream fileStream = File.OpenRead(modFile)) {
				using (BinaryReader reader = new BinaryReader(fileStream)) {
					data = reader.ReadBytes(reader.ReadInt32());
				}
			}

			if (data.Length == 0) {
				return properties;
			}

			using (MemoryStream memoryStream = new MemoryStream(data)) {
				using (BinaryReader reader = new BinaryReader(memoryStream)) {
					for (string tag = reader.ReadString(); tag.Length > 0; tag = reader.ReadString()) {
						if (tag == "dllReferences") {
							List<string> dllReferences = new List<string>();

							for (string reference = reader.ReadString(); reference.Length > 0; reference = reader.ReadString()) {
								dllReferences.Add(reference);
							}

							properties.dllReferences = dllReferences.ToArray();
						}

						if (tag == "modReferences") {
							List<string> modReferences = new List<string>();

							for (string reference = reader.ReadString(); reference.Length > 0; reference = reader.ReadString()) {
								modReferences.Add(reference);
							}

							properties.modReferences = modReferences.ToArray();
						}

						if (tag == "author") {
							properties.author = reader.ReadString();
						}

						if (tag == "version") {
							properties.version = reader.ReadString();
						}

						if (tag == "displayName") {
							properties.displayName = reader.ReadString();
						}
					}
				}
			}

			return properties;
		}

		internal static void Unload() {
			foreach (Mod mod in mods.Values) {
				mod.Unload();
			}

			loadedMods.Clear();
			mods.Clear();
			textures.Clear();
			ResizeArrays(true);
		}

		internal static void Reload() {
			Unload();

			Main.menuMode = LoaderMenus.loadModsID;
		}

		internal static bool IsEnabled(string modFile) {
			string enablePath = Path.ChangeExtension(modFile, "enabled");

			return !File.Exists(enablePath) || File.ReadAllText(enablePath) != "false";
		}

		internal static void SetModActive(string modFile, bool active) {
			string path = Path.ChangeExtension(modFile, "enabled");

			using (StreamWriter writer = File.CreateText(path)) {
				writer.Write(active ? "true" : "false");
			}
		}

		internal static void EnableMod(string modFile) => SetModActive(modFile, true);

		internal static void DisableMod(string modFile) => SetModActive(modFile, false);

		internal static string[] FindModSources() {
			Directory.CreateDirectory(ModSourcesPath);

			return Directory.GetDirectories(ModSourcesPath, "*", SearchOption.TopDirectoryOnly).Where(directory => directory != ".vs").ToArray();
		}

		internal static void BuildAllMods() => ThreadPool.QueueUserWorkItem(new WaitCallback(do_BuildAllMods), 1);

		internal static void do_BuildAllMods(object threadContext) {
			string[] modFolders = FindModSources();
			int modsBuilt = 0;
			bool error = false;

			foreach (string modFolder in modFolders) {
				LoaderMenus.buildMod.SetProgress(modsBuilt, modFolders.Length);

				modToBuild = modFolder;

				if (!do_BuildMod(threadContext)) {
					error = true;
				}

				modsBuilt++;
			}

            Main.menuMode = error ? LoaderMenus.errorMessageID : (reloadAfterBuild ? LoaderMenus.loadModsID : 0);
		}

		internal static void BuildMod() {
			LoaderMenus.buildMod.SetProgress(0, 1);
			ThreadPool.QueueUserWorkItem(new WaitCallback(delegate (object threadContext) { do_BuildMod(threadContext); }), 1);
		}

		internal static bool do_BuildMod(object threadContext) {
			LoaderMenus.buildMod.SetReading();

			BuildProperties properties = ReadBuildProperties(modToBuild);

			if (properties == null) {
				if (!buildAll) {
					Main.menuMode = LoaderMenus.errorMessageID;
				}

				return false;
			}

			LoadReferences();
			LoaderMenus.buildMod.SetCompiling();

			if (!CompileMod(modToBuild, properties)) {
				if (!buildAll) {
					Main.menuMode = LoaderMenus.errorMessageID;
				}

				return false;
			}

			LoaderMenus.buildMod.SetBuildText();

            byte[] propertiesData = PropertiesToBytes(properties);
			string file = ModSavePath + Path.DirectorySeparatorChar + Path.GetFileName(modToBuild) + ".tea";
			byte[] buffer = File.ReadAllBytes(file);

			using (FileStream fileStream = File.Create(file)) {
				using (BinaryWriter writer = new BinaryWriter(fileStream)) {
					writer.Write(propertiesData.Length);
					writer.Write(propertiesData);
					writer.Write(buffer.Length);
					writer.Write(buffer);

					string[] images = Directory.GetFiles(modToBuild, "*.png", SearchOption.AllDirectories);

					foreach (string image in images) {
						string texturePath = image.Replace(ModSourcesPath + Path.DirectorySeparatorChar, null);

						texturePath = Path.ChangeExtension(texturePath.Replace(Path.DirectorySeparatorChar, '/'), null);
						buffer = File.ReadAllBytes(image);
						writer.Write(texturePath);
						writer.Write(buffer.Length);
						writer.Write(buffer);
					}
					writer.Write("end");
				}
			}
			EnableMod(file);

			if (!buildAll) {
				Main.menuMode = reloadAfterBuild ? LoaderMenus.loadModsID : 0;
			}
			return true;
		}

		private static BuildProperties ReadBuildProperties(string modDir) {
			string propertiesFile = modDir + Path.DirectorySeparatorChar + "build.txt";
			BuildProperties properties = new BuildProperties();

			if (!File.Exists(propertiesFile)) {
				return properties;
			}

			string[] lines = File.ReadAllLines(propertiesFile);

			foreach (string line in lines) {
				if (line.Length == 0) {
					continue;
				}

				int split = line.IndexOf('=');
				string property = line.Substring(0, split).Trim();
				string value = line.Substring(split + 1).Trim();

				if (value.Length == 0) {
					continue;
				}

				switch (property) {
					case "dllReferences":
						string[] dllReferences = value.Split(',');
						for (int k = 0; k < dllReferences.Length; k++) {
							string dllReference = dllReferences[k].Trim();
							if (dllReference.Length > 0) {
								dllReferences[k] = dllReference;
							}
						}

						properties.dllReferences = dllReferences;
						break;

					case "modReferences":
						string[] modReferences = value.Split(',');
						for (int k = 0; k < modReferences.Length; k++) {
							string modReference = modReferences[k].Trim();
							if (modReference.Length > 0) {
								modReferences[k] = modReference;
							}
						}

						properties.modReferences = modReferences;
						break;

					case "author":
						properties.author = value;
						break;

					case "version":
						properties.version = value;
						break;

					case "displayName":
						properties.displayName = value;
						break;
				}
			}
			foreach (string modReference in properties.modReferences) {
				string path = ModSavePath + Path.DirectorySeparatorChar + modReference + ".tea";
				if (!File.Exists(path)) {
					ErrorLogger.LogModReferenceError(modReference);

					return null;
				}

				byte[] data;
				using (FileStream fileStream = File.OpenRead(path)) {
					using (BinaryReader reader = new BinaryReader(fileStream)) {
						fileStream.Seek(reader.ReadInt32(), SeekOrigin.Current);
						data = reader.ReadBytes(reader.ReadInt32());
					}
				}

				using (FileStream writeStream = File.Create(ModSourcesPath + Path.DirectorySeparatorChar + modReference + ".dll")) {
					using (BinaryWriter writer = new BinaryWriter(writeStream)) {
						writer.Write(data);
					}
				}
			}

			return properties;
		}

		private static byte[] PropertiesToBytes(BuildProperties properties) {
			byte[] data;

			using (MemoryStream memoryStream = new MemoryStream()) {
				using (BinaryWriter writer = new BinaryWriter(memoryStream)) {
					if (properties.dllReferences.Length > 0) {
						writer.Write("dllReferences");

						foreach (string reference in properties.dllReferences) {
							writer.Write(reference);
						}

						writer.Write("");
					}

					if (properties.modReferences.Length > 0) {
						writer.Write("modReferences");

						foreach (string reference in properties.modReferences) {
							writer.Write(reference);

						}
						writer.Write("");
					}

					if (properties.author.Length > 0) {
						writer.Write("author");
						writer.Write(properties.author);
					}

					if (properties.version.Length > 0) {
						writer.Write("version");
						writer.Write(properties.version);
					}

					if (properties.displayName.Length > 0) {
						writer.Write("displayName");
						writer.Write(properties.displayName);
					}

					writer.Write("");
					writer.Flush();
					data = memoryStream.ToArray();
				}
			}

			return data;
		}

		private static bool CompileMod(string modDir, BuildProperties properties) {
			CompilerParameters compileOptions = new CompilerParameters();
			compileOptions.GenerateExecutable = false;
			compileOptions.GenerateInMemory = false;
			compileOptions.OutputAssembly = ModSavePath + Path.DirectorySeparatorChar + Path.GetFileName(modDir) + ".tea";

			foreach (string reference in buildReferences) {
				compileOptions.ReferencedAssemblies.Add(reference);
			}

			Directory.CreateDirectory(ReferencePath);

			foreach (string reference in properties.dllReferences) {
				compileOptions.ReferencedAssemblies.Add(ReferencePath + Path.DirectorySeparatorChar + reference + ".dll");
			}

			foreach (string reference in properties.modReferences) {
				compileOptions.ReferencedAssemblies.Add(ModSourcesPath + Path.DirectorySeparatorChar + reference + ".dll");
			}

			CodeDomProvider codeProvider = new CSharpCodeProvider();
			CompilerResults results = codeProvider.CompileAssemblyFromFile(compileOptions, Directory.GetFiles(modDir, "*.cs", SearchOption.AllDirectories));
			CompilerErrorCollection errors = results.Errors;

			foreach (string reference in properties.modReferences) {
				File.Delete(ModSourcesPath + Path.DirectorySeparatorChar + reference + ".dll");
			}

			if (errors.HasErrors) {
                ErrorLogger.LogCompileErrors(errors);

				return false;
			}

			return true;
		}

		public static Texture2D GetTexture(string name) {
			if (!textures.ContainsKey(name)) {
				throw new ArgumentException("Missing texture " + name);
			}

			return textures[name];
		}

		internal static void AddRecipes() {
			foreach (Mod mod in mods.Values) {
				try {
					mod.AddRecipes();

					foreach (ModItem item in mod.items.Values) {
						item.AddRecipes();
					}
				}
				catch {
					DisableMod(mod.filePath);

					throw;
				}
			}
		}
	}
}
