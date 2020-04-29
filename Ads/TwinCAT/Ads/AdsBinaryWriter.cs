namespace TwinCAT.Ads
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Text;
    using TwinCAT.PlcOpen;
    using TwinCAT.TypeSystem;

    public class AdsBinaryWriter : BinaryWriter
    {
        public AdsBinaryWriter(AdsStream stream) : base(stream, Encoding.Default)
        {
        }

        public void WriteGuid(Guid guid)
        {
            this.Write(guid.ToByteArray());
        }

        public void WritePlcAnsiString(string value, int length)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            Encoding encoding = Encoding.Default;
            byte[] bytes = null;
            if (value.Length >= length)
            {
                int byteCount = encoding.GetByteCount("a");
                bytes = new byte[length * encoding.GetByteCount("a")];
                int num2 = encoding.GetBytes(value, 0, length, bytes, 0);
                this.Write(bytes);
            }
            else if (value.Length < length)
            {
                bytes = new byte[length * encoding.GetByteCount("a")];
                int num3 = encoding.GetBytes(value, 0, value.Length, bytes, 0);
                this.Write(bytes);
            }
        }

        public void WritePlcAnsiStringFixedLength(string value, int byteSize)
        {
            byte[] buffer = PlcStringConverter.MarshalAnsi(value, StringConvertMode.FixedLengthZeroTerminated, byteSize);
            this.Write(buffer);
        }

        [Obsolete("Use AdsBinaryWriter.WritePlcAnsiString instead!"), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public void WritePlcString(string value, int length)
        {
            this.WritePlcAnsiString(value, length);
        }

        public void WritePlcString(string value, int length, Encoding encoding)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (ReferenceEquals(encoding, Encoding.Default))
            {
                this.WritePlcAnsiString(value, length);
            }
            else if (ReferenceEquals(encoding, Encoding.Unicode))
            {
                this.WritePlcUnicodeString(value, length);
            }
            else
            {
                byte[] bytes = null;
                if (value.Length >= length)
                {
                    string s = value.Substring(0, length);
                    int byteCount = Encoding.Unicode.GetByteCount("a");
                    bytes = new byte[encoding.GetByteCount(s)];
                    int num2 = encoding.GetBytes(value, 0, length, bytes, 0);
                    this.Write(bytes);
                }
                else if (value.Length < length)
                {
                    bytes = new byte[encoding.GetByteCount(value)];
                    int num3 = encoding.GetBytes(value, 0, value.Length, bytes, 0);
                    this.Write(bytes);
                }
            }
        }

        public void WritePlcType(DateTime value)
        {
            this.Write(new DATE(value).Ticks);
        }

        public void WritePlcType(TimeSpan value)
        {
            this.Write(new TIME(value).Ticks);
        }

        public void WritePlcUnicodeString(string value, int length)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            Encoding unicode = Encoding.Unicode;
            byte[] bytes = null;
            if (value.Length < length)
            {
                bytes = new byte[(value.Length + 1) * Encoding.Unicode.GetByteCount("a")];
                int num3 = Encoding.Unicode.GetBytes(value, 0, value.Length, bytes, 0);
                this.Write(bytes);
            }
            else
            {
                int byteCount = unicode.GetByteCount("a");
                bytes = new byte[length * Encoding.Unicode.GetByteCount("a")];
                int num2 = Encoding.Unicode.GetBytes(value, 0, length, bytes, 0);
                this.Write(bytes);
            }
        }

        public void WritePlcUnicodeStringFixedLength(string value, int byteSize)
        {
            byte[] buffer = PlcStringConverter.MarshalUnicode(value, StringConvertMode.FixedLengthZeroTerminated, byteSize);
            this.Write(buffer);
        }
    }
}

