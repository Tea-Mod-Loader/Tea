using System;
using System.IO;

namespace Terraria.ModLoader
{
	public static class Logging
	{
		private static StreamWriter _clientWriter;
		private static StreamWriter _serverWriter;

		public static readonly string LogPath = Main.SavePath + Path.DirectorySeparatorChar + "Logs";

		public static void LogClient(string message) {
			Directory.CreateDirectory(LogPath);

			if (_clientWriter == null)
				_clientWriter = File.AppendText(LogPath + Path.DirectorySeparatorChar + "ClientLog.txt");

			_clientWriter.WriteLine($"[{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}] {message}");
		}

		public static void LogServer(string message) {
			Directory.CreateDirectory(LogPath);

			if (_serverWriter == null)
				_serverWriter = File.AppendText(LogPath + Path.DirectorySeparatorChar + "ServerLog.txt");
			
			_serverWriter.WriteLine($"[{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}] {message}");
		}

		public static void ClearLog() {
			if (!Directory.Exists(LogPath))
				Directory.CreateDirectory(LogPath);

			using (_clientWriter = File.AppendText(LogPath + Path.DirectorySeparatorChar + "ClientLog.txt")) { }
			using (_serverWriter = File.AppendText(LogPath + Path.DirectorySeparatorChar + "ServerLog.txt")) { }
		}
	}
}
