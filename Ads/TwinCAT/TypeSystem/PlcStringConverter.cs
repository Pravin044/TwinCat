namespace TwinCAT.TypeSystem
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Text;
    using TwinCAT.Ads;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public class PlcStringConverter
    {
        private Encoding _encoding;
        private StringConvertMode _mode = StringConvertMode.FixedLengthZeroTerminated;
        private static PlcStringConverter s_default;
        private static PlcStringConverter s_unicode;

        internal PlcStringConverter(Encoding defaultEncoding, StringConvertMode mode)
        {
            this._encoding = defaultEncoding;
            this._mode = mode;
        }

        public static bool CanMarshal(AdsDatatypeId typeId) => 
            ((typeId == AdsDatatypeId.ADST_STRING) || (typeId == AdsDatatypeId.ADST_WSTRING));

        public static bool CanMarshal(DataTypeCategory category) => 
            (category == DataTypeCategory.String);

        public byte[] Marshal(string str)
        {
            int maxBytes = this.MarshalSize(str);
            return Marshal(str, this._encoding, this._mode, maxBytes);
        }

        public byte[] Marshal(string str, int maxBytes) => 
            Marshal(str, this._encoding, this._mode, maxBytes);

        internal static byte[] Marshal(string str, Encoding encoding, StringConvertMode mode, int maxBytes)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            byte[] bytes = null;
            if (mode == StringConvertMode.ZeroTerminated)
            {
                int num = encoding.GetByteCount(str) + encoding.GetByteCount("\0");
                if ((maxBytes >= 0) && (num > maxBytes))
                {
                    throw new ArgumentOutOfRangeException("str");
                }
                bytes = new byte[num];
                encoding.GetBytes(str, 0, str.Length, bytes, 0);
            }
            else if (mode == StringConvertMode.FixedLengthZeroTerminated)
            {
                if (maxBytes < 0)
                {
                    throw new ArgumentException("maxBytes");
                }
                if ((encoding.GetByteCount(str) + encoding.GetByteCount("\0")) > maxBytes)
                {
                    throw new ArgumentOutOfRangeException("str");
                }
                bytes = new byte[maxBytes];
                encoding.GetBytes(str, 0, str.Length, bytes, 0);
            }
            else
            {
                if (mode != StringConvertMode.LengthPrefix)
                {
                    throw new NotSupportedException();
                }
                uint byteCount = (uint) encoding.GetByteCount(str);
                if ((maxBytes >= 0) && (byteCount > maxBytes))
                {
                    throw new ArgumentOutOfRangeException("str");
                }
                bytes = new byte[byteCount + 4];
                Array.Copy(BitConverter.GetBytes(byteCount), bytes, 4);
                encoding.GetBytes(str, 0, str.Length, bytes, 4);
            }
            return bytes;
        }

        public static byte[] MarshalAnsi(string str, int bytes)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            byte[] buffer = new byte[bytes];
            Encoding.Default.GetBytes(str, 0, str.Length, buffer, 0);
            return buffer;
        }

        public static byte[] MarshalAnsi(string str, StringConvertMode mode)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            Encoding encoding = null;
            encoding = Encoding.Default;
            return Marshal(str, encoding, mode, -1);
        }

        public static byte[] MarshalAnsi(string str, StringConvertMode mode, int maxBytes)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            Encoding encoding = null;
            encoding = Encoding.Default;
            return Marshal(str, encoding, mode, maxBytes);
        }

        internal int MarshalSize(int strLen) => 
            MarshalSize(strLen, this._encoding, this._mode);

        public int MarshalSize(string str) => 
            MarshalSize(str, this._encoding, this._mode);

        internal static int MarshalSize(int strLen, Encoding encoding, StringConvertMode mode)
        {
            int byteCount = encoding.GetByteCount("a");
            int num2 = strLen * byteCount;
            switch (mode)
            {
                case StringConvertMode.FixedLength:
                    break;

                case StringConvertMode.FixedLengthZeroTerminated:
                case StringConvertMode.ZeroTerminated:
                    num2 += byteCount;
                    break;

                case StringConvertMode.LengthPrefix:
                    num2 += 4;
                    break;

                default:
                    throw new NotSupportedException();
            }
            return num2;
        }

        internal static int MarshalSize(string str, Encoding encoding, StringConvertMode mode)
        {
            if (encoding == null)
            {
                encoding = Encoding.Default;
            }
            int byteCount = encoding.GetByteCount(str);
            switch (mode)
            {
                case StringConvertMode.FixedLength:
                    break;

                case StringConvertMode.FixedLengthZeroTerminated:
                case StringConvertMode.ZeroTerminated:
                    byteCount += encoding.GetByteCount("\0");
                    break;

                case StringConvertMode.LengthPrefix:
                    byteCount += 4;
                    break;

                default:
                    throw new NotSupportedException();
            }
            return byteCount;
        }

        public static int MarshalSizeAnsi(string str, StringConvertMode mode) => 
            MarshalSize(str, Encoding.Default, mode);

        public static int MarshalSizeUnicode(string str, StringConvertMode mode) => 
            MarshalSize(str, Encoding.Unicode, mode);

        public static byte[] MarshalUnicode(string str, StringConvertMode mode) => 
            Marshal(str, Encoding.Unicode, mode, -1);

        public static byte[] MarshalUnicode(string str, StringConvertMode mode, int maxBytes) => 
            Marshal(str, Encoding.Unicode, mode, maxBytes);

        internal int Unmarshal(byte[] bytes, out string value) => 
            Unmarshal(bytes, 0, bytes.Length, this._encoding, this._mode, out value);

        internal int Unmarshal(byte[] bytes, int offset, int byteCount, out string value) => 
            Unmarshal(bytes, offset, byteCount, this._encoding, this._mode, out value);

        internal static string Unmarshal(byte[] bytes, int index, int byteCount, Encoding encoding, StringConvertMode mode)
        {
            string str = null;
            int num = Unmarshal(bytes, index, byteCount, encoding, mode, out str);
            return str;
        }

        internal static int Unmarshal(byte[] bytes, int offset, int byteCount, Encoding encoding, StringConvertMode mode, out string value)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if ((byteCount < 0) && ((mode == StringConvertMode.FixedLength) || (mode == StringConvertMode.FixedLengthZeroTerminated)))
            {
                throw new ArgumentOutOfRangeException("byteCount");
            }
            value = null;
            if (((mode != StringConvertMode.FixedLength) && (mode != StringConvertMode.FixedLengthZeroTerminated)) && (mode != StringConvertMode.ZeroTerminated))
            {
                if (mode != StringConvertMode.LengthPrefix)
                {
                    throw new NotSupportedException();
                }
                int num4 = (int) BitConverter.ToUInt32(bytes, offset);
                offset += 4;
                value = encoding.GetString(bytes, offset, num4);
                return (num4 + 4);
            }
            StringConvertMode mode1 = mode;
            int count = 0;
            count = (byteCount < 0) ? (bytes.Length - offset) : byteCount;
            char[] chArray = encoding.GetChars(bytes, offset, count);
            int length = chArray.Length;
            if ((mode == StringConvertMode.FixedLengthZeroTerminated) || (mode == StringConvertMode.ZeroTerminated))
            {
                for (int i = 0; i < chArray.Length; i++)
                {
                    if (chArray[i] == '\0')
                    {
                        length = i;
                        break;
                    }
                }
            }
            value = new string(chArray, 0, length);
            return count;
        }

        public static string UnmarshalAnsi(byte[] bytes) => 
            UnmarshalAnsi(bytes, 0, bytes.Length);

        public static string UnmarshalAnsi(byte[] bytes, int index, int byteCount)
        {
            Encoding encoding = Encoding.Default;
            StringConvertMode zeroTerminated = StringConvertMode.ZeroTerminated;
            if (byteCount >= 0)
            {
                zeroTerminated = StringConvertMode.FixedLengthZeroTerminated;
            }
            return Unmarshal(bytes, index, byteCount, encoding, zeroTerminated);
        }

        public static string UnmarshalUnicode(byte[] bytes, int index, int byteCount)
        {
            StringConvertMode zeroTerminated = StringConvertMode.ZeroTerminated;
            if (byteCount >= 0)
            {
                zeroTerminated = StringConvertMode.FixedLengthZeroTerminated;
            }
            return Unmarshal(bytes, index, byteCount, Encoding.Unicode, zeroTerminated);
        }

        public static PlcStringConverter Default
        {
            get
            {
                if (s_default == null)
                {
                    s_default = new PlcStringConverter(Encoding.Default, StringConvertMode.FixedLengthZeroTerminated);
                }
                return s_default;
            }
        }

        public static PlcStringConverter DefaultVariableLength =>
            new PlcStringConverter(Encoding.Default, StringConvertMode.ZeroTerminated);

        public static PlcStringConverter Unicode
        {
            get
            {
                if (s_unicode == null)
                {
                    s_unicode = new PlcStringConverter(Encoding.Unicode, StringConvertMode.FixedLengthZeroTerminated);
                }
                return s_unicode;
            }
        }

        public static PlcStringConverter UnicodeVariableLength =>
            new PlcStringConverter(Encoding.Unicode, StringConvertMode.ZeroTerminated);
    }
}

