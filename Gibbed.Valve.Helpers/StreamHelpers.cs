using System;
using System.IO;
using System.Text;

namespace Gibbed.Valve.Helpers
{
	public static class StreamHelpers
	{
		/// <summary>
		/// Read an 8-bit boolean value.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static bool ReadBoolean(this Stream stream)
		{
			return stream.ReadU8() > 0 ? true : false;
		}

		public static void WriteBoolean(this Stream stream, bool value)
		{
			stream.WriteU8((byte)(value == true ? 1 : 0));
		}

		/// <summary>
		/// Read an unsigned 8-bit integer.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static byte ReadU8(this Stream stream)
		{
			return (byte)stream.ReadByte();
		}

		public static void WriteU8(this Stream stream, byte value)
		{
			stream.WriteByte(value);
		}

		/// <summary>
		/// Read a signed 8-bit integer;
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static char ReadS8(this Stream stream)
		{
			return (char)stream.ReadByte();
		}

		public static void WriteS8(this Stream stream, char value)
		{
			stream.WriteByte((byte)value);
		}

		/// <summary>
		/// Read a signed 16-bit integer.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static Int16 ReadS16(this Stream stream)
		{
			byte[] data = new byte[2];
			stream.Read(data, 0, 2);
			return BitConverter.ToInt16(data, 0);
		}

		public static void WriteS16(this Stream stream, Int16 value)
		{
			byte[] data = BitConverter.GetBytes(value);
			stream.Write(data, 0, 2);
		}

		/// <summary>
		/// Read an unsigned 16-bit integer.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static UInt16 ReadU16(this Stream stream)
		{
			byte[] data = new byte[2];
			stream.Read(data, 0, 2);
			return BitConverter.ToUInt16(data, 0);
		}

		public static void WriteU16(this Stream stream, UInt16 value)
		{
			byte[] data = BitConverter.GetBytes(value);
			stream.Write(data, 0, 2);
		}

		/// <summary>
		/// Read a signed 32-bit integer.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static Int32 ReadS32(this Stream stream)
		{
			byte[] data = new byte[4];
			stream.Read(data, 0, 4);
			return BitConverter.ToInt32(data, 0);
		}

		public static void WriteS32(this Stream stream, Int32 value)
		{
			byte[] data = BitConverter.GetBytes(value);
			stream.Write(data, 0, 4);
		}

		/// <summary>
		/// Read an unsigned 32-bit integer.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static UInt32 ReadU32(this Stream stream)
		{
			byte[] data = new byte[4];
			stream.Read(data, 0, 4);
			return BitConverter.ToUInt32(data, 0);
		}

		public static void WriteU32(this Stream stream, UInt32 value)
		{
			byte[] data = BitConverter.GetBytes(value);
			stream.Write(data, 0, 4);
		}

		/// <summary>
		/// Read a signed 64-bit integer.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static Int64 ReadS64(this Stream stream)
		{
			byte[] data = new byte[8];
			stream.Read(data, 0, 8);
			return BitConverter.ToInt64(data, 0);
		}

		/// <summary>
		/// Read an unsigned 64-bit integer.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static UInt64 ReadU64(this Stream stream)
		{
			byte[] data = new byte[8];
			stream.Read(data, 0, 8);
			return BitConverter.ToUInt64(data, 0);
		}

		/// <summary>
		/// Read a 32-bit floating point number.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static Single ReadF32(this Stream stream)
		{
			byte[] data = new byte[4];
			stream.Read(data, 0, 4);
			return BitConverter.ToSingle(data, 0);
		}

		public static void WriteF32(this Stream stream, Single value)
		{
			byte[] data = BitConverter.GetBytes(value);
			stream.Write(data, 0, 4);
		}

		/// <summary>
		/// Read a 64-bit floating point number.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static Double ReadF64(this Stream stream)
		{
			byte[] data = new byte[8];
			stream.Read(data, 0, 8);
			return BitConverter.ToDouble(data, 0);
		}

		public static void WriteF64(this Stream stream, Double value)
		{
			byte[] data = BitConverter.GetBytes(value);
			stream.Write(data, 0, 8);
		}

		public static string ReadASCII(this Stream stream, uint size)
		{
			byte[] data = new byte[size];
			stream.Read(data, 0, data.Length);
			return Encoding.ASCII.GetString(data);
		}

		public static string ReadASCIIZ(this Stream stream)
		{
			int i = 0;
			byte[] data = new byte[64];

			while (true)
			{
				stream.Read(data, i, 1);
				if (data[i] == 0)
				{
					break;
				}

				if (i >= data.Length)
				{
					if (data.Length >= 4096)
					{
						throw new InvalidOperationException();
					}

					Array.Resize(ref data, data.Length + 64);
				}

				i++;
			}

			if (i == 0)
			{
				return "";
			}

			return Encoding.ASCII.GetString(data, 0, i);
		}

		public static void WriteASCII(this Stream stream, string value)
		{
			byte[] data = Encoding.ASCII.GetBytes(value);
			stream.Write(data, 0, data.Length);
		}
	}
}
