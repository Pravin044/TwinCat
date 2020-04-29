namespace TwinCAT.Ads.Internal
{
    using System;
    using System.ComponentModel;
    using System.Text;
    using TwinCAT.Ads;
    using TwinCAT.TypeSystem;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never), Browsable(false)]
    public sealed class AdsSymbolEntry : ISymbolInfo, IAdsCustomMarshal<AdsSymbolEntry>
    {
        public uint entryLength;
        public uint indexGroup;
        public uint indexOffset;
        public uint size;
        public uint dataType;
        public AdsSymbolFlags flags;
        public uint extendedFlags;
        public ushort arrayDim;
        public ushort nameLength;
        public ushort typeLength;
        public ushort commentLength;
        public string name;
        public string type;
        public string comment;
        public AdsDatatypeArrayInfo[] array;
        public Guid typeGuid;
        public ushort attributeCount;
        public AdsAttributeEntry[] attributes;
        public byte[] reserved;

        public AdsSymbolEntry(long parentEndPosition, Encoding encoding, AdsBinaryReader reader)
        {
            this.Read(parentEndPosition, encoding, reader);
        }

        public void Read(long parentEndPosition, Encoding encoding, AdsBinaryReader reader)
        {
            long position = reader.BaseStream.Position;
            this.entryLength = reader.ReadUInt32();
            long num2 = position + this.entryLength;
            this.indexGroup = reader.ReadUInt32();
            this.indexOffset = reader.ReadUInt32();
            this.size = reader.ReadUInt32();
            this.dataType = reader.ReadUInt32();
            ushort num3 = reader.ReadUInt16();
            this.flags = (AdsSymbolFlags) num3;
            this.arrayDim = reader.ReadUInt16();
            this.nameLength = reader.ReadUInt16();
            this.typeLength = reader.ReadUInt16();
            this.commentLength = reader.ReadUInt16();
            this.name = reader.ReadPlcString(this.nameLength + 1, encoding);
            this.type = reader.ReadPlcString(this.typeLength + 1, encoding);
            this.comment = reader.ReadPlcString(this.commentLength + 1, encoding);
            this.array = SubStructureReader<AdsDatatypeArrayInfo>.Read(this.arrayDim, num2, encoding, reader);
            if (this.flags.HasFlag(AdsSymbolFlags.None | AdsSymbolFlags.TypeGuid))
            {
                this.typeGuid = new Guid(reader.ReadBytes(0x10));
            }
            if (this.flags.HasFlag(AdsSymbolFlags.Attributes))
            {
                this.attributeCount = reader.ReadUInt16();
                this.attributes = SubStructureReader<AdsAttributeEntry>.Read(this.attributeCount, num2, encoding, reader);
            }
            if (this.flags.HasFlag(AdsSymbolFlags.ExtendedFlags))
            {
                this.extendedFlags = reader.ReadUInt32();
            }
            if (reader.BaseStream.Position != num2)
            {
                if (reader.BaseStream.Position <= num2)
                {
                    this.reserved = reader.ReadBytes((int) (num2 - reader.BaseStream.Position));
                }
                else
                {
                    object[] args = new object[] { this.name, this.type };
                    Module.Trace.TraceError("Reading Symbol entry for '{0}:{1}' failed!", args);
                    reader.BaseStream.Position = num2;
                }
            }
        }

        public byte ContextMask =>
            ((byte) (((ushort) (this.flags & 0xf00)) >> 8));

        string ISymbolInfo.InstancePath =>
            this.name;

        string ISymbolInfo.TypeName =>
            this.type;
    }
}

