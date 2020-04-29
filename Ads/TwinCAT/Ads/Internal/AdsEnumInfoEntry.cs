namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Text;
    using TwinCAT.Ads;

    internal class AdsEnumInfoEntry : IAdsEnumCustomMarshal<AdsEnumInfoEntry>
    {
        public byte nameLength;
        public string name;
        public byte[] value;

        public AdsEnumInfoEntry()
        {
        }

        internal AdsEnumInfoEntry(uint valueSize, long parentEndPosition, Encoding encoding, AdsBinaryReader reader)
        {
            this.Read(valueSize, parentEndPosition, encoding, reader);
        }

        public void Read(uint valueSize, long parentEndPosition, Encoding encoding, AdsBinaryReader reader)
        {
            this.nameLength = reader.ReadByte();
            this.name = reader.ReadPlcString(this.nameLength + 1, encoding);
            this.value = reader.ReadBytes((int) valueSize);
        }
    }
}

