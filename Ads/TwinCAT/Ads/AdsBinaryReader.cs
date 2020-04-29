namespace TwinCAT.Ads
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Text;
    using TwinCAT.PlcOpen;
    using TwinCAT.TypeSystem;

    public class AdsBinaryReader : BinaryReader
    {
        public AdsBinaryReader(AdsStream stream) : base(stream, Encoding.Default)
        {
        }

        public Guid ReadGuid() => 
            new Guid(this.ReadBytes(0x10));

        public string ReadPlcAnsiString(int byteLength)
        {
            byte[] bytes = this.ReadBytes(byteLength);
            return PlcStringConverter.UnmarshalAnsi(bytes, 0, bytes.Length);
        }

        public DateTime ReadPlcDATE() => 
            new DATE(this.ReadUInt32()).Date;

        [Obsolete("Use AdsBinaryReader.ReadPlcAnsiString(int) instead!"), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public string ReadPlcString(int byteLength) => 
            this.ReadPlcAnsiString(byteLength);

        public string ReadPlcString(int byteLength, Encoding encoding)
        {
            byte[] bytes = this.ReadBytes(byteLength);
            return PlcStringConverter.Unmarshal(bytes, 0, bytes.Length, encoding, StringConvertMode.FixedLengthZeroTerminated);
        }

        public TimeSpan ReadPlcTIME() => 
            new TIME(this.ReadUInt32()).Time;

        public string ReadPlcUnicodeString(int byteLength)
        {
            byte[] bytes = this.ReadBytes(byteLength);
            return PlcStringConverter.UnmarshalUnicode(bytes, 0, bytes.Length);
        }
    }
}

