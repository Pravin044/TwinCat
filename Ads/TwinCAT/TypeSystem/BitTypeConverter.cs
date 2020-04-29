namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    internal static class BitTypeConverter
    {
        internal static byte[] Marshal(int bitSize, object value)
        {
            if (bitSize < 1)
            {
                throw new ArgumentOutOfRangeException("bitSize");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            byte[] array = null;
            if (value.GetType() != typeof(BitArray))
            {
                array = PrimitiveTypeConverter.Default.Marshal(value);
            }
            else
            {
                array = new byte[(bitSize / 8) + 1];
                ((BitArray) value).CopyTo(array, 0);
            }
            return array;
        }

        internal static BitArray ToBinary<T>(T numeric) where T: IConvertible
        {
            Type type = typeof(T);
            byte[] bytes = null;
            if (numeric is int)
            {
                bytes = BitConverter.GetBytes((int) numeric);
            }
            else if (type == typeof(long))
            {
                bytes = BitConverter.GetBytes((long) numeric);
            }
            else if (type == typeof(uint))
            {
                bytes = BitConverter.GetBytes((uint) numeric);
            }
            else if (type == typeof(ulong))
            {
                bytes = BitConverter.GetBytes((ulong) numeric);
            }
            else if (type == typeof(short))
            {
                bytes = BitConverter.GetBytes((short) numeric);
            }
            else if (type == typeof(ushort))
            {
                bytes = BitConverter.GetBytes((ushort) numeric);
            }
            else if (type == typeof(byte))
            {
                bytes = BitConverter.GetBytes((short) ((byte) numeric));
            }
            else
            {
                if (type != typeof(sbyte))
                {
                    throw new ArgumentException("Type '{0}' not supported!", type.Name);
                }
                bytes = BitConverter.GetBytes((short) ((sbyte) numeric));
            }
            return new BitArray(bytes);
        }

        internal static T ToNumeric<T>(BitArray binary) where T: IConvertible
        {
            if (binary == null)
            {
                throw new ArgumentNullException("binary");
            }
            Type dataType = typeof(T);
            int num = PrimitiveTypeConverter.MarshalSize(dataType);
            int num2 = (binary.Length / 8) + (((binary.Length % 8) > 0) ? 1 : 0);
            if (num < num2)
            {
                throw new ArgumentException($"Target type must be at least '{num2}' bytes long!", "binary");
            }
            byte[] array = new byte[num];
            binary.CopyTo(array, 0);
            object val = null;
            PrimitiveTypeConverter.Default.UnmarshalPrimitive(dataType, array, 0, -1, out val);
            return (T) val;
        }

        internal static object ToNumeric(Type tp, BitArray binary)
        {
            object obj2 = null;
            if (tp == typeof(int))
            {
                obj2 = ToNumeric<int>(binary);
            }
            else if (tp == typeof(long))
            {
                obj2 = ToNumeric<long>(binary);
            }
            else if (tp == typeof(uint))
            {
                obj2 = ToNumeric<uint>(binary);
            }
            else if (tp == typeof(ulong))
            {
                obj2 = ToNumeric<int>(binary);
            }
            else if (tp == typeof(short))
            {
                obj2 = ToNumeric<short>(binary);
            }
            else if (tp == typeof(ushort))
            {
                obj2 = ToNumeric<ushort>(binary);
            }
            else if (tp == typeof(byte))
            {
                obj2 = ToNumeric<byte>(binary);
            }
            else
            {
                if (tp != typeof(sbyte))
                {
                    throw new ArgumentException("Type '{0}' not supported!", tp.Name);
                }
                obj2 = ToNumeric<sbyte>(binary);
            }
            return obj2;
        }

        internal static int Unmarshal(int bitSize, byte[] data, int bitOffset, out object val)
        {
            if (bitSize < 1)
            {
                throw new ArgumentOutOfRangeException("bitSize");
            }
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            if (data.Length == 0)
            {
                throw new ArgumentOutOfRangeException("data");
            }
            int num = 0;
            if ((data.Length * 8) < (bitSize + bitOffset))
            {
                throw new ArgumentException("Data array to small!", "data");
            }
            BitArray array = new BitArray(data);
            BitArray array2 = new BitArray(bitSize);
            int num3 = 0;
            while (true)
            {
                if (num3 >= bitSize)
                {
                    if (bitSize == 1)
                    {
                        val = array2[0];
                    }
                    else if ((bitSize % 8) != 0)
                    {
                        val = array2;
                    }
                    else
                    {
                        byte[] buffer = new byte[((array2.Length - 1) / 8) + 1];
                        array2.CopyTo(buffer, 0);
                        PrimitiveTypeConverter converter = PrimitiveTypeConverter.Default;
                        if (bitSize <= 0x10)
                        {
                            if (bitSize == 8)
                            {
                                num = converter.UnmarshalPrimitive(typeof(byte), buffer, 0, -1, out val);
                                break;
                            }
                            if (bitSize == 0x10)
                            {
                                num = converter.UnmarshalPrimitive(typeof(ushort), buffer, 0, -1, out val);
                                break;
                            }
                        }
                        else
                        {
                            if (bitSize == 0x20)
                            {
                                num = converter.UnmarshalPrimitive(typeof(uint), buffer, 0, -1, out val);
                                break;
                            }
                            if (bitSize == 0x40)
                            {
                                num = converter.UnmarshalPrimitive(typeof(ulong), buffer, 0, -1, out val);
                                break;
                            }
                        }
                        val = buffer;
                    }
                    break;
                }
                array2[num3] = array[num3 + bitOffset];
                num3++;
            }
            return num;
        }
    }
}

