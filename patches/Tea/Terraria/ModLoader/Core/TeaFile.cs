using Ionic.Zlib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Core
{
	public class TeaFile : IEnumerable<TeaFile.FileEntry>
	{
		public class FileEntry
		{
			public string Name { get; }

			public int Offset { get; internal set; }

			public int Length { get; }
			public int CompressedLength { get; }

			internal byte[] cachedBytes;

			internal FileEntry(string name, int offset, int length, int compressedLength, byte[] cachedBytes = null) {
				Name = name;
				Offset = offset;
				Length = length;
				CompressedLength = compressedLength;
				this.cachedBytes = cachedBytes;
			}

			public bool IsCompressed => Length != CompressedLength;
		}

		public const uint MIN_COMPRESS_SIZE = 1 << 10;
		public const uint MAX_CACHE_SIZE = 1 << 17;
		public const float COMPRESSION_TRADEOFF = 0.9f;

		private static string Sanitize(string path) => path.Replace('\\', '/');

		public readonly string path;

		private FileStream fileStream;
		private IDictionary<string, FileEntry> files = new Dictionary<string, FileEntry>();
		private FileEntry[] fileTable;

		private int openCounter;
		private EntryReadStream sharedEntryReadStream;
		private List<EntryReadStream> independentEntryReadStreams = new List<EntryReadStream>();

		public Version teaVersion { get; private set; }

		public string name { get; private set; }

		public Version version { get; private set; }

		public byte[] hash { get; private set; }

		internal byte[] signature { get; private set; } = new byte[256];

		internal TeaFile(string path, string name = null, Version version = null) {
			this.path = path;
			this.name = name;
			this.version = version;
		}

		public bool HasFile(string fileName) => files.ContainsKey(Sanitize(fileName));

		public byte[] GetBytes(FileEntry entry) {
			if (entry.cachedBytes != null && !entry.IsCompressed) {
				return entry.cachedBytes;
			}

			using (Stream stream = GetStream(entry)) {
				return stream.ReadBytes(entry.Length);
			}
		}

		public byte[] GetBytes(string fileName) => files.TryGetValue(Sanitize(fileName), out FileEntry entry) ? GetBytes(entry) : null;

		public Stream GetStream(FileEntry entry, bool newFileStream = false) {
			Stream stream;

			if (entry.cachedBytes != null) {
				stream = new MemoryStream(entry.cachedBytes);
			}
			else if (fileStream == null) {
				throw new IOException($"File not open: {path}");
			}
			else if (newFileStream) {
				EntryReadStream ers = new EntryReadStream(this, entry, File.OpenRead(path), false);
				independentEntryReadStreams.Add(ers);
				stream = ers;
			}
			else if (sharedEntryReadStream != null) {
				throw new IOException($"Previous entry read stream not closed: {sharedEntryReadStream.Name}");
			}
			else {
				stream = sharedEntryReadStream = new EntryReadStream(this, entry, fileStream, true);
			}

			if (entry.IsCompressed) {
				stream = new DeflateStream(stream, CompressionMode.Decompress);
			}

			return stream;
		}

		internal void OnStreamClosed(EntryReadStream stream) {
			if (stream == sharedEntryReadStream) {
				sharedEntryReadStream = null;
			}
			else if (!independentEntryReadStreams.Remove(stream)) {
				throw new IOException($"Closed EntryReadStream not associated with this file. {stream.Name} @ {path}");
			}
		}

		public Stream GetStream(string fileName, bool newFileStream = false) {
			if (!files.TryGetValue(Sanitize(fileName), out FileEntry entry)) {
				throw new KeyNotFoundException(fileName);
			}

			return GetStream(entry, newFileStream);
		}

		internal void AddFile(string fileName, byte[] data) {
			fileName = Sanitize(fileName);
			int size = data.Length;

			if (size > MIN_COMPRESS_SIZE && ShouldCompress(fileName)) {
				using (MemoryStream ms = new MemoryStream(data.Length)) {
					using (DeflateStream ds = new DeflateStream(ms, CompressionMode.Compress)) {
						ds.Write(data, 0, data.Length);
					}

					byte[] compressed = ms.ToArray();
					if (compressed.Length < size * COMPRESSION_TRADEOFF) {
						data = compressed;
					}
				}
			}

			lock (files) {
				files[fileName] = new FileEntry(fileName, -1, size, data.Length, data);
			}

			fileTable = null;
		}

		internal void RemoveFile(string fileName) {
			files.Remove(Sanitize(fileName));
			fileTable = null;
		}

		public int Count => fileTable.Length;

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public IEnumerator<FileEntry> GetEnumerator() {
			foreach (FileEntry entry in fileTable) {
				yield return entry;
			}
		}

		internal void Save() {
			if (fileStream != null) {
				throw new IOException($"File already open: {path}");
			}

			using (fileStream = File.Create(path))
			using (BinaryWriter writer = new BinaryWriter(fileStream)) {
				writer.Write(Encoding.ASCII.GetBytes("TEA"));
				writer.Write((teaVersion = ModLoader.version).ToString());

				int hashPos = (int)fileStream.Position;
				writer.Write(new byte[20 + 256 + 4]);

				int dataPos = (int)fileStream.Position;
				writer.Write(name);
				writer.Write(version.ToString());

				fileTable = files.Values.ToArray();
				writer.Write(fileTable.Length);

				foreach (FileEntry f in fileTable) {
					if (f.CompressedLength != f.cachedBytes.Length) {
						throw new Exception($"CompressedLength ({f.CompressedLength}) != cachedBytes.Length ({f.cachedBytes.Length}): {f.Name}");
					}

					writer.Write(f.Name);
					writer.Write(f.Length);
					writer.Write(f.CompressedLength);
				}

				int offset = (int)fileStream.Position;
				foreach (FileEntry f in fileTable) {
					writer.Write(f.cachedBytes);

					f.Offset = offset;
					offset += f.CompressedLength;
				}

				fileStream.Position = dataPos;
				hash = SHA1.Create().ComputeHash(fileStream);

				fileStream.Position = hashPos;
				writer.Write(hash);

				fileStream.Seek(256, SeekOrigin.Current);

				writer.Write((int)(fileStream.Length - dataPos));
			}
			fileStream = null;
		}

		private class DisposeWrapper : IDisposable
		{
			private readonly Action dispose;

			public DisposeWrapper(Action dispose) {
				this.dispose = dispose;
			}

			public void Dispose() => dispose?.Invoke();
		}

		public IDisposable Open() {
			if (openCounter++ == 0) {
				if (fileStream != null) {
					throw new Exception($"File already opened? {path}");
				}

				try {
					if (name == null) {
						Read();
					}
					else {
						Reopen();
					}
				}
				catch {
					try { Close(); }
					catch { }
					throw;
				}
			}

			return new DisposeWrapper(Close);
		}

		private void Close() {
			if (openCounter == 0) {
				return;
			}

			if (--openCounter == 0) {
				if (sharedEntryReadStream != null) {
					throw new IOException($"Previous entry read stream not closed: {sharedEntryReadStream.Name}");
				}

				if (independentEntryReadStreams.Count != 0) {
					throw new IOException($"Shared entry read streams not closed: {string.Join(", ", independentEntryReadStreams.Select(e => e.Name))}");
				}

				fileStream?.Close();
				fileStream = null;
			}
		}

		public bool IsOpen => fileStream != null;

		private static bool ShouldCompress(string fileName) =>
			!fileName.EndsWith(".png") &&
			!fileName.EndsWith(".mp3") &&
			!fileName.EndsWith(".ogg");

		private void Read() {
			fileStream = File.OpenRead(path);
			BinaryReader reader = new BinaryReader(fileStream);

			if (Encoding.ASCII.GetString(reader.ReadBytes(4)) != "TEA") {
				throw new Exception("Magic Header != \"TEA\"");
			}

			teaVersion = new Version(reader.ReadString());
			hash = reader.ReadBytes(20);
			signature = reader.ReadBytes(256);
			int datalen = reader.ReadInt32();

			long pos = fileStream.Position;
			byte[] verifyHash = SHA1.Create().ComputeHash(fileStream);
			if (!verifyHash.SequenceEqual(hash)) {
				throw new Exception(Language.GetTextValue("tModLoader.LoadErrorHashMismatchCorrupted"));
			}

			fileStream.Position = pos;

			name = reader.ReadString();
			version = new Version(reader.ReadString());

			int offset = 0;
			fileTable = new FileEntry[reader.ReadInt32()];
			for (int i = 0; i < fileTable.Length; i++) {
				FileEntry f = new FileEntry(
					reader.ReadString(),
					offset,
					reader.ReadInt32(),
					reader.ReadInt32());
				fileTable[i] = f;
				files[f.Name] = f;

				offset += f.CompressedLength;
			}

			int fileStartPos = (int)fileStream.Position;
			foreach (FileEntry f in fileTable) {
				f.Offset += fileStartPos;
			}
		}

		private void Reopen() {
			fileStream = File.OpenRead(path);
			BinaryReader reader = new BinaryReader(fileStream);

			if (Encoding.ASCII.GetString(reader.ReadBytes(4)) != "TEA") {
				throw new Exception("Magic Header != \"TEA\"");
			}

			reader.ReadString();

			if (!reader.ReadBytes(20).SequenceEqual(hash)) {
				throw new Exception($"File has been modifed, hash. {path}");
			}
		}

		public void CacheFiles(ISet<string> skip = null) {
			fileStream.Seek(fileTable[0].Offset, SeekOrigin.Begin);

			foreach (FileEntry entry in fileTable) {
				if (entry.CompressedLength > MAX_CACHE_SIZE || (skip?.Contains(entry.Name) ?? false)) {
					fileStream.Seek(entry.CompressedLength, SeekOrigin.Current);

					continue;
				}

				entry.cachedBytes = fileStream.ReadBytes(entry.CompressedLength);
			}
		}

		public void RemoveFromCache(IEnumerable<string> fileNames) {
			foreach (string fileName in fileNames) {
				files[fileName].cachedBytes = null;
			}
		}

		public void ResetCache() {
			foreach (FileEntry entry in fileTable) {
				entry.cachedBytes = null;
			}
		}
	}
}