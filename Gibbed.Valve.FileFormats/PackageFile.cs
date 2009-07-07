using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Gibbed.Helpers;

namespace Gibbed.Valve.FileFormats
{
    public class PackageEntry
    {
        public string FileName;
        public string DirectoryName;
        public string TypeName;
        public UInt32 CRC32;
        public UInt32 Size;
        public UInt32 Offset;
        public UInt16 ArchiveIndex;
        public byte[] SmallData;
    }

    public class PackageFile
    {
        public List<PackageEntry> Entries;

        public void Serialize(Stream output)
        {
            throw new NotImplementedException();
        }

        public void Deserialize(Stream input)
        {
            if (input.ReadU32() != 0x55AA1234)
            {
                input.Seek(-4, SeekOrigin.Current);
            }
            else
            {
                uint version = input.ReadU32();
                uint indexSize = input.ReadU32();

                if (version != 1)
                {
                    throw new FormatException("unexpected version " + version.ToString());
                }
            }

            List<PackageEntry> entries = new List<PackageEntry>();

            // Types
            while (true)
            {
                string typeName = input.ReadASCIIZ();
                if (typeName == "")
                {
                    break;
                }

                // Directories
                while (true)
                {
                    string directoryName = input.ReadASCIIZ();
                    if (directoryName == "")
                    {
                        break;
                    }

                    // Files
                    while (true)
                    {
                        string fileName = input.ReadASCIIZ();
                        if (fileName == "")
                        {
                            break;
                        }

                        PackageEntry entry = new PackageEntry();
                        entry.FileName = fileName;
                        entry.DirectoryName = directoryName.Replace("/", "\\");
                        entry.TypeName = typeName;
                        entry.CRC32 = input.ReadU32();
                        entry.SmallData = new byte[input.ReadU16()];
                        entry.ArchiveIndex = input.ReadU16();
                        entry.Offset = input.ReadU32();
                        entry.Size = input.ReadU32();
                        
                        UInt16 terminator = input.ReadU16();
                        if (terminator != 0xFFFF)
                        {
                            throw new FormatException("invalid terminator");
                        }

                        if (entry.SmallData.Length > 0)
                        {
                            input.Read(entry.SmallData, 0, entry.SmallData.Length);
                        }

                        entries.Add(entry);
                    }
                }
            }

            this.Entries = entries;
        }
    }
}
