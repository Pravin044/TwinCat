namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Text;
    using TwinCAT.Ads;

    internal class AdsMethodEntry : IAdsCustomMarshal<AdsMethodEntry>
    {
        internal uint entryLength;
        internal uint version;
        internal uint vTableIndex;
        internal uint returnSize;
        internal uint returnAlignSize;
        internal uint reserved;
        internal Guid returnTypeGuid;
        internal uint returnDataType;
        internal uint flags;
        internal ushort nameLength;
        internal ushort returnTypeLength;
        internal ushort commentLength;
        internal ushort parameterCount;
        internal string name;
        internal string returnType;
        internal string comment;
        internal AdsMethodParaInfo[] parameters;

        public AdsMethodEntry()
        {
        }

        internal AdsMethodEntry(long parentEndPosition, Encoding encoding, AdsBinaryReader reader)
        {
            this.Read(parentEndPosition, encoding, reader);
        }

        public void Read(long parentEndPosition, Encoding encoding, AdsBinaryReader reader)
        {
            long position = reader.BaseStream.Position;
            this.entryLength = reader.ReadUInt32();
            long num2 = position + this.entryLength;
            this.version = reader.ReadUInt32();
            this.vTableIndex = reader.ReadUInt32();
            this.returnSize = reader.ReadUInt32();
            this.returnAlignSize = reader.ReadUInt32();
            this.reserved = reader.ReadUInt32();
            this.returnTypeGuid = reader.ReadGuid();
            this.returnDataType = reader.ReadUInt32();
            this.flags = reader.ReadUInt32();
            this.nameLength = reader.ReadUInt16();
            this.returnTypeLength = reader.ReadUInt16();
            this.commentLength = reader.ReadUInt16();
            this.parameterCount = reader.ReadUInt16();
            this.name = reader.ReadPlcString(this.nameLength + 1, encoding);
            this.returnType = reader.ReadPlcString(this.returnTypeLength + 1, encoding);
            this.comment = reader.ReadPlcString(this.commentLength + 1, encoding);
            this.parameters = SubStructureReader<AdsMethodParaInfo>.Read(this.parameterCount, num2, encoding, reader);
            if (reader.BaseStream.Position != num2)
            {
                if (reader.BaseStream.Position <= num2)
                {
                    byte[] buffer = reader.ReadBytes((int) (num2 - reader.BaseStream.Position));
                }
                else
                {
                    reader.BaseStream.Position = num2;
                }
            }
        }
    }
}

