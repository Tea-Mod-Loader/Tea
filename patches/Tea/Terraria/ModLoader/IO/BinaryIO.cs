using System.IO;

namespace Terraria.ModLoader.IO
{
	public static class BinaryIO
	{
		public static void ReadBytes(this Stream stream, byte[] buf) {
			int r = 0;
			int pos = 0;

			while ((r = stream.Read(buf, pos, buf.Length - pos)) > 0) {
				pos += r;
			}

			if (pos != buf.Length) {
				throw new IOException($"Stream did not contain enough bytes ({pos}) < ({buf.Length})");
			}
		}

		public static byte[] ReadBytes(this Stream stream, int len) => ReadBytes(stream, (long)len);

		public static byte[] ReadBytes(this Stream stream, long len) {
			byte[] buf = new byte[len];

			stream.ReadBytes(buf);

			return buf;
		}
	}
}
