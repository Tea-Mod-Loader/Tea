using System;
using System.IO;

namespace Terraria.ModLoader
{
	public static class Logging
	{
		public static readonly string LogPath = Main.SavePath + Path.DirectorySeparatorChar + "Logs";

		public static void LogClient(string message) {
			Directory.CreateDirectory(LogPath);

            using (StreamWriter writer = File.AppendText(LogPath + Path.DirectorySeparatorChar + "logs.txt"))
				try {
                    writer.WriteLine($"[{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}] {message}");
				}
				catch {

				}
		}

		public static void ClearLog() {
			if (!Directory.Exists(LogPath))
				Directory.CreateDirectory(LogPath);

			using (StreamWriter writer = File.AppendText(LogPath + Path.DirectorySeparatorChar + "serverlogs.txt")) { }
		}
	}
}
