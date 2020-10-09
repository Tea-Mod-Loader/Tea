using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;

namespace Terraria.Tea
{
	public static class ErrorLogger
	{
		public static readonly string LogPath = Main.SavePath + Path.DirectorySeparatorChar + "Logs";

		internal static void LogModReferenceError(string reference) {
			Directory.CreateDirectory(LogPath);
			string file = LogPath + Path.DirectorySeparatorChar + "Compile Errors.txt";
			string message = "Mod reference " + reference + " was not found.";
			using (StreamWriter writer = File.CreateText(file)) {
				writer.WriteLine(message);
			}
			LoaderMenus.errorMessage.SetMessage(message);
			LoaderMenus.errorMessage.SetGotoMenu(LoaderMenus.modSourcesID);
			LoaderMenus.errorMessage.SetFile(file);
		}

		internal static void LogCompileErrors(CompilerErrorCollection errors) {
			Directory.CreateDirectory(LogPath);
			string file = LogPath + Path.DirectorySeparatorChar + "Compile Errors.txt";
			using (StreamWriter writer = File.CreateText(file)) {
				foreach (CompilerError error in errors) {
					writer.WriteLine(error.ToString());
					writer.WriteLine();
				}
			}
			LoaderMenus.errorMessage.SetMessage("An error occurred while compiling a mod.\n\n" + errors[0]);
			LoaderMenus.errorMessage.SetGotoMenu(LoaderMenus.modSourcesID);
			LoaderMenus.errorMessage.SetFile(file);
		}

		internal static void LogMissingLoadReference(List<string> mods) {
			Directory.CreateDirectory(LogPath);
			string file = LogPath + Path.DirectorySeparatorChar + "Loading Errors.txt";
			string message = "The following mods were missing mod dependencies. They have been automatically disabled.\n";
			foreach (string modFile in mods) {
				message += Path.GetFileNameWithoutExtension(modFile) + "\n";
			}
			using (StreamWriter writer = File.CreateText(file)) {
				writer.Write(message);
			}
			LoaderMenus.errorMessage.SetMessage(message);
			LoaderMenus.errorMessage.SetGotoMenu(LoaderMenus.reloadModsID);
			LoaderMenus.errorMessage.SetFile(file);
		}

		internal static void LogLoadingError(string modFile, Exception e) {
			Directory.CreateDirectory(LogPath);
			string file = LogPath + Path.DirectorySeparatorChar + "Loading Errors.txt";
			using (StreamWriter writer = File.CreateText(file)) {
				writer.WriteLine(e.Message);
				writer.WriteLine(e.StackTrace);
			}
			string message = "An error occurred while loading " + Path.GetFileNameWithoutExtension(modFile);
			if (modFile != "recipes") {
				message += "\nThis mod has automatically been disabled.";
			}
			message += "\n\n" + e.Message + "\n" + e.StackTrace;
			LoaderMenus.errorMessage.SetMessage(message);
			LoaderMenus.errorMessage.SetGotoMenu(LoaderMenus.reloadModsID);
			LoaderMenus.errorMessage.SetFile(file);
		}

		internal static void LogException(Exception e) {
			Directory.CreateDirectory(LogPath);
			string file = LogPath + Path.DirectorySeparatorChar + "Runtime Error.txt";
			using (StreamWriter writer = File.CreateText(file)) {
				writer.WriteLine(e.Message);
				writer.WriteLine(e.StackTrace);
			}
			LoaderMenus.errorMessage.SetMessage("The game has crashed!\n\n" + e.Message + "\n" + e.StackTrace);
			LoaderMenus.errorMessage.SetGotoMenu(0);
			LoaderMenus.errorMessage.SetFile(file);
			Main.gameMenu = true;
			Main.menuMode = LoaderMenus.errorMessageID;
		}

		public static void Log(string message) {
			Directory.CreateDirectory(LogPath);
			using (StreamWriter writer = File.AppendText(LogPath + Path.DirectorySeparatorChar + "Logs.txt")) {
				writer.WriteLine(message);
			}
		}

		public static void ClearLog() {
			Directory.CreateDirectory(LogPath);
			using (StreamWriter writer = File.CreateText(LogPath + Path.DirectorySeparatorChar + "Logs.txt")) {
			}
		}
	}
}
