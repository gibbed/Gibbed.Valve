using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Gibbed.Helpers;

namespace Gibbed.Valve.Package
{
	public class PackageFileException : Exception
	{
	}

	public class PackageEntry
	{
		public string FileName;
		public string DirectoryName;
		public string TypeName;
		public UInt32 HashOrTimestamp;
		public UInt32 Size;
		public UInt32 Offset;
		public UInt16 ArchiveIndex;
		public byte[] SmallData;
		public UInt16 Unknown;
	}

	public class PackageFile
	{
		public List<PackageEntry> Entries;

		public void Deserialize(Stream stream)
		{
			List<PackageEntry> entries = new List<PackageEntry>();

			// Types
			while (true)
			{
				string typeName = stream.ReadASCIIZ();
				if (typeName == "")
				{
					break;
				}

				// Directories
				while (true)
				{
					string directoryName = stream.ReadASCIIZ();
					if (directoryName == "")
					{
						break;
					}

					// Files
					while (true)
					{
						string fileName = stream.ReadASCIIZ();
						if (fileName == "")
						{
							break;
						}

						PackageEntry entry = new PackageEntry();
						entry.FileName = fileName;
						entry.DirectoryName = directoryName.Replace("/", "\\");
						entry.TypeName = typeName;
						entry.HashOrTimestamp = stream.ReadU32();
						entry.SmallData = new byte[stream.ReadU16()];
						entry.ArchiveIndex = stream.ReadU16();
						entry.Offset = stream.ReadU32();
						entry.Size = stream.ReadU32();
						entry.Unknown = stream.ReadU16();

						if (entry.Unknown != 0xFFFF)
						{
							throw new PackageFileException();
						}

						if (entry.SmallData.Length > 0)
						{
							stream.Read(entry.SmallData, 0, entry.SmallData.Length);
						}

						entries.Add(entry);
					}
				}
			}

			this.Entries = entries;
		}
	}
}
