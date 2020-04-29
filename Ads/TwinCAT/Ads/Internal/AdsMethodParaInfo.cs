namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Text;
    using TwinCAT.Ads;
    using TwinCAT.TypeSystem;

    internal class AdsMethodParaInfo : IAdsCustomMarshal<AdsMethodParaInfo>
    {
        internal uint entryLength;
        internal uint size;
        internal uint alignSize;
        internal AdsDatatypeId dataType;
        internal MethodParamFlags flags;
        internal uint reserved;
        internal Guid typeGuid;
        internal ushort lengthIsPara;
        internal ushort nameLength;
        internal ushort typeLength;
        internal ushort commentLength;
        internal string name;
        internal string type;
        internal string comment;

        public AdsMethodParaInfo()
        {
        }

        internal AdsMethodParaInfo(long parentEndPosition, Encoding encoding, AdsBinaryReader reader)
        {
            this.Read(parentEndPosition, encoding, reader);
        }

        public void Read(long parentEndPosition, Encoding encoding, AdsBinaryReader reader)
        {
            long position = reader.BaseStream.Position;
            this.entryLength = reader.ReadUInt32();
            long num2 = position + this.entryLength;
            this.size = reader.ReadUInt32();
            this.alignSize = reader.ReadUInt32();
            this.dataType = (AdsDatatypeId) reader.ReadUInt32();
            this.flags = (MethodParamFlags) reader.ReadUInt32();
            this.reserved = reader.ReadUInt32();
            this.typeGuid = reader.ReadGuid();
            this.lengthIsPara = reader.ReadUInt16();
            this.nameLength = reader.ReadUInt16();
            this.typeLength = reader.ReadUInt16();
            this.commentLength = reader.ReadUInt16();
            this.name = reader.ReadPlcString(this.nameLength + 1, encoding);
            this.type = reader.ReadPlcString(this.typeLength + 1, encoding);
            this.comment = reader.ReadPlcString(this.commentLength + 1, encoding);
            if (reader.BaseStream.Position <= num2)
            {
                byte[] buffer = reader.ReadBytes((int) (num2 - reader.BaseStream.Position));
            }
            else if (reader.BaseStream.Position > num2)
            {
                object[] args = new object[] { this.name };
                Module.Trace.TraceError("Reading MethodPara entry for '{0}' failed!", args);
                reader.BaseStream.Position = num2;
            }
        }
    }
}

