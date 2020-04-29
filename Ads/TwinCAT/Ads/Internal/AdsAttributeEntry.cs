namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Text;
    using TwinCAT.Ads;
    using TwinCAT.TypeSystem;

    public class AdsAttributeEntry : ITypeAttribute, IAdsCustomMarshal<AdsAttributeEntry>
    {
        private string _name;
        private string _value;

        public AdsAttributeEntry()
        {
        }

        internal AdsAttributeEntry(long parentEndPosition, Encoding encoding, AdsBinaryReader reader)
        {
            this.Read(parentEndPosition, encoding, reader);
        }

        public void Read(long parentEndPosition, Encoding encoding, AdsBinaryReader reader)
        {
            byte num = reader.ReadByte();
            byte num2 = reader.ReadByte();
            this._name = reader.ReadPlcString(num + 1, encoding);
            this._value = reader.ReadPlcString(num2 + 1, encoding);
        }

        public string Name =>
            this._name;

        public string Value =>
            this._value;
    }
}

