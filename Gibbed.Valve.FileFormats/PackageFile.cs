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
            if (input.ReadValueU32() != 0x55AA1234)
            {
                input.Seek(-4, SeekOrigin.Current);
            }
            else
            {
                uint version = input.ReadValueU32();
                uint indexSize = input.ReadValueU32();

                if (version != 1)
                {
                    throw new FormatException("unexpected version " + version.ToString());
                }
            }

            List<PackageEntry> entries = new List<PackageEntry>();

            // Types
            while (true)
            {
                string typeName = input.ReadStringASCIIZ();
                if (typeName == "")
                {
                    break;
                }

                // Directories
                while (true)
                {
                    string directoryName = input.ReadStringASCIIZ();
                    if (directoryName == "")
                    {
                        break;
                    }

                    // Files
                    while (true)
                    {
                        string fileName = input.ReadStringASCIIZ();
                        if (fileName == "")
                        {
                            break;
                        }

                        PackageEntry entry = new PackageEntry();
                        entry.FileName = fileName;
                        entry.DirectoryName = directoryName.Replace("/", "\\");
                        entry.TypeName = typeName;
                        entry.CRC32 = input.ReadValueU32();
                        entry.SmallData = new byte[input.ReadValueU16()];
                        entry.ArchiveIndex = input.ReadValueU16();
                        entry.Offset = input.ReadValueU32();
                        entry.Size = input.ReadValueU32();
                        
                        UInt16 terminator = input.ReadValueU16();
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
