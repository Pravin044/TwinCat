namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using TwinCAT.Ads;
    using TwinCAT.PlcOpen;

    internal class TcAdsDllMarshaller
    {
        public static IntPtr Alloc(int size) => 
            Marshal.AllocHGlobal(size);

        public static void Free(IntPtr ptr)
        {
            Marshal.FreeHGlobal(ptr);
        }

        public static unsafe AdsVersion PtrToAdsVersion(byte* ptr)
        {
            AdsVersion version = new AdsVersion();
            ptr++;
            version.Version = ptr[0];
            ptr++;
            version.Revision = ptr[0];
            version.Build = *((ushort*) ptr);
            return version;
        }

        [SecuritySafeCritical]
        private static unsafe object PtrToArrayOfBoolean(void* ptr, int[] elements, ref int size)
        {
            switch (elements.Length)
            {
                case 1:
                {
                    bool[] flagArray2;
                    int num2 = elements[0];
                    if (ptr == null)
                    {
                        size = num2;
                        return null;
                    }
                    if (num2 < size)
                    {
                        throw new ArgumentException();
                    }
                    size = num2;
                    bool[] flagArray = new bool[elements[0]];
                    if (((flagArray2 = flagArray) == null) || (flagArray2.Length == 0))
                    {
                        flagRef = null;
                    }
                    else
                    {
                        flagRef = flagArray2;
                    }
                    UnsafeNativeMethods.memcpy((void*) flagRef, ptr, size);
                    fixed (bool* flagRef = null)
                    {
                        return flagArray;
                    }
                }
                case 2:
                {
                    bool[,] flagArray4;
                    int num3 = elements[0] * elements[1];
                    if (ptr == null)
                    {
                        size = num3;
                        return null;
                    }
                    if (num3 < size)
                    {
                        throw new ArgumentException();
                    }
                    size = num3;
                    bool[,] flagArray3 = new bool[elements[0], elements[1]];
                    if (((flagArray4 = flagArray3) == null) || (flagArray4.Length == 0))
                    {
                        flagRef2 = null;
                    }
                    else
                    {
                        flagRef2 = (bool*) flagArray4[0, 0];
                    }
                    UnsafeNativeMethods.memcpy((void*) flagRef2, ptr, size);
                    fixed (bool* flagRef2 = null)
                    {
                        return flagArray3;
                    }
                }
                case 3:
                {
                    bool[,,] flagArray6;
                    int num4 = (elements[0] * elements[1]) * elements[2];
                    if (ptr == null)
                    {
                        size = num4;
                        return null;
                    }
                    if (num4 < size)
                    {
                        throw new ArgumentException();
                    }
                    size = num4;
                    bool[,,] flagArray5 = new bool[elements[0], elements[1], elements[2]];
                    if (((flagArray6 = flagArray5) == null) || (flagArray6.Length == 0))
                    {
                        flagRef3 = null;
                    }
                    else
                    {
                        flagRef3 = (bool*) flagArray6[0, 0, 0];
                    }
                    UnsafeNativeMethods.memcpy((void*) flagRef3, ptr, size);
                    fixed (bool* flagRef3 = null)
                    {
                        return flagArray5;
                    }
                }
            }
            throw new ArgumentException();
        }

        [SecuritySafeCritical]
        private static unsafe object PtrToArrayOfInt16(void* ptr, int[] elements, ref int size)
        {
            switch (elements.Length)
            {
                case 1:
                {
                    short[] numArray2;
                    int num2 = elements[0] * 2;
                    if (ptr == null)
                    {
                        size = num2;
                        return null;
                    }
                    if (num2 < size)
                    {
                        throw new ArgumentException();
                    }
                    size = num2;
                    short[] numArray = new short[elements[0]];
                    if (((numArray2 = numArray) == null) || (numArray2.Length == 0))
                    {
                        numRef = null;
                    }
                    else
                    {
                        numRef = numArray2;
                    }
                    UnsafeNativeMethods.memcpy((void*) numRef, ptr, size);
                    fixed (short* numRef = null)
                    {
                        return numArray;
                    }
                }
                case 2:
                {
                    short[,] numArray4;
                    int num3 = (elements[0] * elements[1]) * 2;
                    if (ptr == null)
                    {
                        size = num3;
                        return null;
                    }
                    if (num3 < size)
                    {
                        throw new ArgumentException();
                    }
                    size = num3;
                    short[,] numArray3 = new short[elements[0], elements[1]];
                    if (((numArray4 = numArray3) == null) || (numArray4.Length == 0))
                    {
                        numRef2 = null;
                    }
                    else
                    {
                        numRef2 = (short*) numArray4[0, 0];
                    }
                    UnsafeNativeMethods.memcpy((void*) numRef2, ptr, size);
                    fixed (short* numRef2 = null)
                    {
                        return numArray3;
                    }
                }
                case 3:
                {
                    short[,,] numArray6;
                    int num4 = ((elements[0] * elements[1]) * elements[2]) * 2;
                    if (ptr == null)
                    {
                        size = num4;
                        return null;
                    }
                    if (num4 < size)
                    {
                        throw new ArgumentException();
                    }
                    size = num4;
                    short[,,] numArray5 = new short[elements[0], elements[1], elements[2]];
                    if (((numArray6 = numArray5) == null) || (numArray6.Length == 0))
                    {
                        numRef3 = null;
                    }
                    else
                    {
                        numRef3 = (short*) numArray6[0, 0, 0];
                    }
                    UnsafeNativeMethods.memcpy((void*) numRef3, ptr, size);
                    fixed (short* numRef3 = null)
                    {
                        return numArray5;
                    }
                }
            }
            throw new ArgumentException();
        }

        [SecuritySafeCritical]
        private static unsafe object PtrToArrayOfInt32(void* ptr, int[] elements, ref int size)
        {
            switch (elements.Length)
            {
                case 1:
                {
                    int[] numArray2;
                    int num2 = elements[0] * 4;
                    if (ptr == null)
                    {
                        size = num2;
                        return null;
                    }
                    if (num2 < size)
                    {
                        throw new ArgumentException();
                    }
                    size = num2;
                    int[] numArray = new int[elements[0]];
                    if (((numArray2 = numArray) == null) || (numArray2.Length == 0))
                    {
                        numRef = null;
                    }
                    else
                    {
                        numRef = numArray2;
                    }
                    UnsafeNativeMethods.memcpy((void*) numRef, ptr, size);
                    fixed (int* numRef = null)
                    {
                        return numArray;
                    }
                }
                case 2:
                {
                    int[,] numArray4;
                    int num3 = (elements[0] * elements[1]) * 4;
                    if (ptr == null)
                    {
                        size = num3;
                        return null;
                    }
                    if (num3 < size)
                    {
                        throw new ArgumentException();
                    }
                    size = num3;
                    int[,] numArray3 = new int[elements[0], elements[1]];
                    if (((numArray4 = numArray3) == null) || (numArray4.Length == 0))
                    {
                        numRef2 = null;
                    }
                    else
                    {
                        numRef2 = (int*) numArray4[0, 0];
                    }
                    UnsafeNativeMethods.memcpy((void*) numRef2, ptr, size);
                    fixed (int* numRef2 = null)
                    {
                        return numArray3;
                    }
                }
                case 3:
                {
                    int[,,] numArray6;
                    int num4 = ((elements[0] * elements[1]) * elements[2]) * 4;
                    if (ptr == null)
                    {
                        size = num4;
                        return null;
                    }
                    if (num4 < size)
                    {
                        throw new ArgumentException();
                    }
                    size = num4;
                    int[,,] numArray5 = new int[elements[0], elements[1], elements[2]];
                    if (((numArray6 = numArray5) == null) || (numArray6.Length == 0))
                    {
                        numRef3 = null;
                    }
                    else
                    {
                        numRef3 = (int*) numArray6[0, 0, 0];
                    }
                    UnsafeNativeMethods.memcpy((void*) numRef3, ptr, size);
                    fixed (int* numRef3 = null)
                    {
                        return numArray5;
                    }
                }
            }
            throw new ArgumentException();
        }

        [SecuritySafeCritical]
        private static unsafe object PtrToArrayOfInt64(void* ptr, int[] elements, ref int size)
        {
            switch (elements.Length)
            {
                case 1:
                {
                    long[] numArray2;
                    int num2 = elements[0] * 8;
                    if (ptr == null)
                    {
                        size = num2;
                        return null;
                    }
                    if (num2 < size)
                    {
                        throw new ArgumentException();
                    }
                    size = num2;
                    long[] numArray = new long[elements[0]];
                    if (((numArray2 = numArray) == null) || (numArray2.Length == 0))
                    {
                        numRef = null;
                    }
                    else
                    {
                        numRef = numArray2;
                    }
                    UnsafeNativeMethods.memcpy((void*) numRef, ptr, size);
                    fixed (long* numRef = null)
                    {
                        return numArray;
                    }
                }
                case 2:
                {
                    long[,] numArray4;
                    int num3 = (elements[0] * elements[1]) * 8;
                    if (ptr == null)
                    {
                        size = num3;
                        return null;
                    }
                    if (num3 < size)
                    {
                        throw new ArgumentException();
                    }
                    size = num3;
                    long[,] numArray3 = new long[elements[0], elements[1]];
                    if (((numArray4 = numArray3) == null) || (numArray4.Length == 0))
                    {
                        numRef2 = null;
                    }
                    else
                    {
                        numRef2 = (long*) numArray4[0, 0];
                    }
                    UnsafeNativeMethods.memcpy((void*) numRef2, ptr, size);
                    fixed (long* numRef2 = null)
                    {
                        return numArray3;
                    }
                }
                case 3:
                {
                    long[,,] numArray6;
                    int num4 = ((elements[0] * elements[1]) * elements[2]) * 8;
                    if (ptr == null)
                    {
                        size = num4;
                        return null;
                    }
                    if (num4 < size)
                    {
                        throw new ArgumentException();
                    }
                    size = num4;
                    long[,,] numArray5 = new long[elements[0], elements[1], elements[2]];
                    if (((numArray6 = numArray5) == null) || (numArray6.Length == 0))
                    {
                        numRef3 = null;
                    }
                    else
                    {
                        numRef3 = (long*) numArray6[0, 0, 0];
                    }
                    UnsafeNativeMethods.memcpy((void*) numRef3, ptr, size);
                    fixed (long* numRef3 = null)
                    {
                        return numArray5;
                    }
                }
            }
            throw new ArgumentException();
        }

        [SecuritySafeCritical]
        private static unsafe object PtrToArrayOfInt8(void* ptr, int[] elements, ref int size)
        {
            switch (elements.Length)
            {
                case 1:
                {
                    sbyte[] numArray2;
                    int num2 = elements[0];
                    if (ptr == null)
                    {
                        size = num2;
                        return null;
                    }
                    if (num2 < size)
                    {
                        throw new ArgumentException();
                    }
                    size = num2;
                    sbyte[] numArray = new sbyte[elements[0]];
                    if (((numArray2 = numArray) == null) || (numArray2.Length == 0))
                    {
                        numRef = null;
                    }
                    else
                    {
                        numRef = numArray2;
                    }
                    UnsafeNativeMethods.memcpy((void*) numRef, ptr, size);
                    fixed (sbyte* numRef = null)
                    {
                        return numArray;
                    }
                }
                case 2:
                {
                    sbyte[,] numArray4;
                    int num3 = elements[0] * elements[1];
                    if (ptr == null)
                    {
                        size = num3;
                        return null;
                    }
                    if (num3 < size)
                    {
                        throw new ArgumentException();
                    }
                    size = num3;
                    sbyte[,] numArray3 = new sbyte[elements[0], elements[1]];
                    if (((numArray4 = numArray3) == null) || (numArray4.Length == 0))
                    {
                        numRef2 = null;
                    }
                    else
                    {
                        numRef2 = (sbyte*) numArray4[0, 0];
                    }
                    UnsafeNativeMethods.memcpy((void*) numRef2, ptr, size);
                    fixed (sbyte* numRef2 = null)
                    {
                        return numArray3;
                    }
                }
                case 3:
                {
                    sbyte[,,] numArray6;
                    int num4 = (elements[0] * elements[1]) * elements[2];
                    if (ptr == null)
                    {
                        size = num4;
                        return null;
                    }
                    if (num4 < size)
                    {
                        throw new ArgumentException();
                    }
                    size = num4;
                    sbyte[,,] numArray5 = new sbyte[elements[0], elements[1], elements[2]];
                    if (((numArray6 = numArray5) == null) || (numArray6.Length == 0))
                    {
                        numRef3 = null;
                    }
                    else
                    {
                        numRef3 = (sbyte*) numArray6[0, 0, 0];
                    }
                    UnsafeNativeMethods.memcpy((void*) numRef3, ptr, size);
                    fixed (sbyte* numRef3 = null)
                    {
                        return numArray5;
                    }
                }
            }
            throw new ArgumentException();
        }

        [SecuritySafeCritical]
        private static unsafe object PtrToArrayOfReal32(void* ptr, int[] elements, ref int size)
        {
            switch (elements.Length)
            {
                case 1:
                {
                    float[] numArray2;
                    int num2 = elements[0] * 4;
                    if (ptr == null)
                    {
                        size = num2;
                        return null;
                    }
                    if (num2 < size)
                    {
                        throw new ArgumentException();
                    }
                    size = num2;
                    float[] numArray = new float[elements[0]];
                    if (((numArray2 = numArray) == null) || (numArray2.Length == 0))
                    {
                        numRef = null;
                    }
                    else
                    {
                        numRef = numArray2;
                    }
                    UnsafeNativeMethods.memcpy((void*) numRef, ptr, size);
                    fixed (float* numRef = null)
                    {
                        return numArray;
                    }
                }
                case 2:
                {
                    float[,] numArray4;
                    int num3 = (elements[0] * elements[1]) * 4;
                    if (ptr == null)
                    {
                        size = num3;
                        return null;
                    }
                    if (num3 < size)
                    {
                        throw new ArgumentException();
                    }
                    size = num3;
                    float[,] numArray3 = new float[elements[0], elements[1]];
                    if (((numArray4 = numArray3) == null) || (numArray4.Length == 0))
                    {
                        numRef2 = null;
                    }
                    else
                    {
                        numRef2 = (float*) numArray4[0, 0];
                    }
                    UnsafeNativeMethods.memcpy((void*) numRef2, ptr, size);
                    fixed (float* numRef2 = null)
                    {
                        return numArray3;
                    }
                }
                case 3:
                {
                    float[,,] numArray6;
                    int num4 = ((elements[0] * elements[1]) * elements[2]) * 4;
                    if (ptr == null)
                    {
                        size = num4;
                        return null;
                    }
                    if (num4 < size)
                    {
                        throw new ArgumentException();
                    }
                    size = num4;
                    float[,,] numArray5 = new float[elements[0], elements[1], elements[2]];
                    if (((numArray6 = numArray5) == null) || (numArray6.Length == 0))
                    {
                        numRef3 = null;
                    }
                    else
                    {
                        numRef3 = (float*) numArray6[0, 0, 0];
                    }
                    UnsafeNativeMethods.memcpy((void*) numRef3, ptr, size);
                    fixed (float* numRef3 = null)
                    {
                        return numArray5;
                    }
                }
            }
            throw new ArgumentException();
        }

        [SecuritySafeCritical]
        private static unsafe object PtrToArrayOfReal64(void* ptr, int[] elements, ref int size)
        {
            switch (elements.Length)
            {
                case 1:
                {
                    double[] numArray2;
                    int num2 = elements[0] * 8;
                    if (ptr == null)
                    {
                        size = num2;
                        return null;
                    }
                    if (num2 < size)
                    {
                        throw new ArgumentException();
                    }
                    size = num2;
                    double[] numArray = new double[elements[0]];
                    if (((numArray2 = numArray) == null) || (numArray2.Length == 0))
                    {
                        numRef = null;
                    }
                    else
                    {
                        numRef = numArray2;
                    }
                    UnsafeNativeMethods.memcpy((void*) numRef, ptr, size);
                    fixed (double* numRef = null)
                    {
                        return numArray;
                    }
                }
                case 2:
                {
                    double[,] numArray4;
                    int num3 = (elements[0] * elements[1]) * 8;
                    if (ptr == null)
                    {
                        size = num3;
                        return null;
                    }
                    if (num3 < size)
                    {
                        throw new ArgumentException();
                    }
                    size = num3;
                    double[,] numArray3 = new double[elements[0], elements[1]];
                    if (((numArray4 = numArray3) == null) || (numArray4.Length == 0))
                    {
                        numRef2 = null;
                    }
                    else
                    {
                        numRef2 = (double*) numArray4[0, 0];
                    }
                    UnsafeNativeMethods.memcpy((void*) numRef2, ptr, size);
                    fixed (double* numRef2 = null)
                    {
                        return numArray3;
                    }
                }
                case 3:
                {
                    double[,,] numArray6;
                    int num4 = ((elements[0] * elements[1]) * elements[2]) * 8;
                    if (ptr == null)
                    {
                        size = num4;
                        return null;
                    }
                    if (num4 < size)
                    {
                        throw new ArgumentException();
                    }
                    size = num4;
                    double[,,] numArray5 = new double[elements[0], elements[1], elements[2]];
                    if (((numArray6 = numArray5) == null) || (numArray6.Length == 0))
                    {
                        numRef3 = null;
                    }
                    else
                    {
                        numRef3 = (double*) numArray6[0, 0, 0];
                    }
                    UnsafeNativeMethods.memcpy((void*) numRef3, ptr, size);
                    fixed (double* numRef3 = null)
                    {
                        return numArray5;
                    }
                }
            }
            throw new ArgumentException();
        }

        private static unsafe string[] PtrToArrayOfString(void* ptr, int characters, int elements, ref int size)
        {
            int num = (characters + 1) * elements;
            if (ptr == null)
            {
                size = num;
                return null;
            }
            if (num < size)
            {
                throw new ArgumentException();
            }
            string[] strArray = new string[elements];
            for (int i = 0; i < elements; i++)
            {
                strArray[i] = PtrToStringAnsi(new IntPtr(ptr + (i * (characters + 1))), characters + 1);
                int index = strArray[i].IndexOf('\0');
                if (index != -1)
                {
                    strArray[i] = strArray[i].Substring(0, index);
                }
            }
            return strArray;
        }

        private static unsafe object PtrToArrayOfStruct(void* ptr, Type elementType, int elements, ref int size)
        {
            Array array = Array.CreateInstance(elementType, elements);
            int num = Marshal.SizeOf(elementType);
            int num2 = num * elements;
            if (ptr == null)
            {
                size = num2;
                return null;
            }
            if (num2 < size)
            {
                throw new ArgumentException();
            }
            size = num2;
            for (int i = 0; i < elements; i++)
            {
                array.SetValue(Marshal.PtrToStructure(new IntPtr(ptr + (i * num)), elementType), i);
            }
            return array;
        }

        [SecuritySafeCritical]
        private static unsafe object PtrToArrayOfUInt16(void* ptr, int[] elements, ref int size)
        {
            switch (elements.Length)
            {
                case 1:
                {
                    ushort[] numArray2;
                    int num2 = elements[0] * 2;
                    if (ptr == null)
                    {
                        size = num2;
                        return null;
                    }
                    if (num2 < size)
                    {
                        throw new ArgumentException();
                    }
                    size = num2;
                    ushort[] numArray = new ushort[elements[0]];
                    if (((numArray2 = numArray) == null) || (numArray2.Length == 0))
                    {
                        numRef = null;
                    }
                    else
                    {
                        numRef = numArray2;
                    }
                    UnsafeNativeMethods.memcpy((void*) numRef, ptr, size);
                    fixed (ushort* numRef = null)
                    {
                        return numArray;
                    }
                }
                case 2:
                {
                    ushort[,] numArray4;
                    int num3 = (elements[0] * elements[1]) * 2;
                    if (ptr == null)
                    {
                        size = num3;
                        return null;
                    }
                    if (num3 < size)
                    {
                        throw new ArgumentException();
                    }
                    size = num3;
                    ushort[,] numArray3 = new ushort[elements[0], elements[1]];
                    if (((numArray4 = numArray3) == null) || (numArray4.Length == 0))
                    {
                        numRef2 = null;
                    }
                    else
                    {
                        numRef2 = (ushort*) numArray4[0, 0];
                    }
                    UnsafeNativeMethods.memcpy((void*) numRef2, ptr, size);
                    fixed (ushort* numRef2 = null)
                    {
                        return numArray3;
                    }
                }
                case 3:
                {
                    ushort[,,] numArray6;
                    int num4 = ((elements[0] * elements[1]) * elements[2]) * 2;
                    if (ptr == null)
                    {
                        size = num4;
                        return null;
                    }
                    if (num4 < size)
                    {
                        throw new ArgumentException();
                    }
                    size = num4;
                    ushort[,,] numArray5 = new ushort[elements[0], elements[1], elements[2]];
                    if (((numArray6 = numArray5) == null) || (numArray6.Length == 0))
                    {
                        numRef3 = null;
                    }
                    else
                    {
                        numRef3 = (ushort*) numArray6[0, 0, 0];
                    }
                    UnsafeNativeMethods.memcpy((void*) numRef3, ptr, size);
                    fixed (ushort* numRef3 = null)
                    {
                        return numArray5;
                    }
                }
            }
            throw new ArgumentException();
        }

        [SecuritySafeCritical]
        private static unsafe object PtrToArrayOfUInt32(void* ptr, int[] elements, ref int size)
        {
            switch (elements.Length)
            {
                case 1:
                {
                    uint[] numArray2;
                    int num2 = elements[0] * 4;
                    if (ptr == null)
                    {
                        size = num2;
                        return null;
                    }
                    if (num2 < size)
                    {
                        throw new ArgumentException();
                    }
                    size = num2;
                    uint[] numArray = new uint[elements[0]];
                    if (((numArray2 = numArray) == null) || (numArray2.Length == 0))
                    {
                        numRef = null;
                    }
                    else
                    {
                        numRef = numArray2;
                    }
                    UnsafeNativeMethods.memcpy((void*) numRef, ptr, size);
                    fixed (uint* numRef = null)
                    {
                        return numArray;
                    }
                }
                case 2:
                {
                    uint[,] numArray4;
                    int num3 = (elements[0] * elements[1]) * 4;
                    if (ptr == null)
                    {
                        size = num3;
                        return null;
                    }
                    if (num3 < size)
                    {
                        throw new ArgumentException();
                    }
                    size = num3;
                    uint[,] numArray3 = new uint[elements[0], elements[1]];
                    if (((numArray4 = numArray3) == null) || (numArray4.Length == 0))
                    {
                        numRef2 = null;
                    }
                    else
                    {
                        numRef2 = (uint*) numArray4[0, 0];
                    }
                    UnsafeNativeMethods.memcpy((void*) numRef2, ptr, size);
                    fixed (uint* numRef2 = null)
                    {
                        return numArray3;
                    }
                }
                case 3:
                {
                    uint[,,] numArray6;
                    int num4 = ((elements[0] * elements[1]) * elements[2]) * 4;
                    if (ptr == null)
                    {
                        size = num4;
                        return null;
                    }
                    if (num4 < size)
                    {
                        throw new ArgumentException();
                    }
                    size = num4;
                    uint[,,] numArray5 = new uint[elements[0], elements[1], elements[2]];
                    if (((numArray6 = numArray5) == null) || (numArray6.Length == 0))
                    {
                        numRef3 = null;
                    }
                    else
                    {
                        numRef3 = (uint*) numArray6[0, 0, 0];
                    }
                    UnsafeNativeMethods.memcpy((void*) numRef3, ptr, size);
                    fixed (uint* numRef3 = null)
                    {
                        return numArray5;
                    }
                }
            }
            throw new ArgumentException();
        }

        [SecuritySafeCritical]
        private static unsafe object PtrToArrayOfUInt64(void* ptr, int[] elements, ref int size)
        {
            switch (elements.Length)
            {
                case 1:
                {
                    ulong[] numArray2;
                    int num2 = elements[0] * 8;
                    if (ptr == null)
                    {
                        size = num2;
                        return null;
                    }
                    if (num2 < size)
                    {
                        throw new ArgumentException();
                    }
                    size = num2;
                    ulong[] numArray = new ulong[elements[0]];
                    if (((numArray2 = numArray) == null) || (numArray2.Length == 0))
                    {
                        numRef = null;
                    }
                    else
                    {
                        numRef = numArray2;
                    }
                    UnsafeNativeMethods.memcpy((void*) numRef, ptr, size);
                    fixed (ulong* numRef = null)
                    {
                        return numArray;
                    }
                }
                case 2:
                {
                    ulong[,] numArray4;
                    int num3 = (elements[0] * elements[1]) * 8;
                    if (ptr == null)
                    {
                        size = num3;
                        return null;
                    }
                    if (num3 < size)
                    {
                        throw new ArgumentException();
                    }
                    size = num3;
                    ulong[,] numArray3 = new ulong[elements[0], elements[1]];
                    if (((numArray4 = numArray3) == null) || (numArray4.Length == 0))
                    {
                        numRef2 = null;
                    }
                    else
                    {
                        numRef2 = (ulong*) numArray4[0, 0];
                    }
                    UnsafeNativeMethods.memcpy((void*) numRef2, ptr, size);
                    fixed (ulong* numRef2 = null)
                    {
                        return numArray3;
                    }
                }
                case 3:
                {
                    ulong[,,] numArray6;
                    int num4 = ((elements[0] * elements[1]) * elements[2]) * 8;
                    if (ptr == null)
                    {
                        size = num4;
                        return null;
                    }
                    if (num4 < size)
                    {
                        throw new ArgumentException();
                    }
                    size = num4;
                    ulong[,,] numArray5 = new ulong[elements[0], elements[1], elements[2]];
                    if (((numArray6 = numArray5) == null) || (numArray6.Length == 0))
                    {
                        numRef3 = null;
                    }
                    else
                    {
                        numRef3 = (ulong*) numArray6[0, 0, 0];
                    }
                    UnsafeNativeMethods.memcpy((void*) numRef3, ptr, size);
                    fixed (ulong* numRef3 = null)
                    {
                        return numArray5;
                    }
                }
            }
            throw new ArgumentException();
        }

        [SecuritySafeCritical]
        private static unsafe object PtrToArrayOfUInt8(void* ptr, int[] elements, ref int size)
        {
            switch (elements.Length)
            {
                case 1:
                {
                    byte[] buffer2;
                    int num2 = elements[0];
                    if (ptr == null)
                    {
                        size = num2;
                        return null;
                    }
                    if (num2 < size)
                    {
                        throw new ArgumentException();
                    }
                    size = num2;
                    byte[] buffer = new byte[elements[0]];
                    if (((buffer2 = buffer) == null) || (buffer2.Length == 0))
                    {
                        numRef = null;
                    }
                    else
                    {
                        numRef = buffer2;
                    }
                    UnsafeNativeMethods.memcpy((void*) numRef, ptr, size);
                    fixed (byte* numRef = null)
                    {
                        return buffer;
                    }
                }
                case 2:
                {
                    byte[,] buffer4;
                    int num3 = elements[0] * elements[1];
                    if (ptr == null)
                    {
                        size = num3;
                        return null;
                    }
                    if (num3 < size)
                    {
                        throw new ArgumentException();
                    }
                    size = num3;
                    byte[,] buffer3 = new byte[elements[0], elements[1]];
                    if (((buffer4 = buffer3) == null) || (buffer4.Length == 0))
                    {
                        numRef2 = null;
                    }
                    else
                    {
                        numRef2 = (byte*) buffer4[0, 0];
                    }
                    UnsafeNativeMethods.memcpy((void*) numRef2, ptr, size);
                    fixed (byte* numRef2 = null)
                    {
                        return buffer3;
                    }
                }
                case 3:
                {
                    byte[,,] buffer6;
                    int num4 = (elements[0] * elements[1]) * elements[2];
                    if (ptr == null)
                    {
                        size = num4;
                        return null;
                    }
                    if (num4 < size)
                    {
                        throw new ArgumentException();
                    }
                    size = num4;
                    byte[,,] buffer5 = new byte[elements[0], elements[1], elements[2]];
                    if (((buffer6 = buffer5) == null) || (buffer6.Length == 0))
                    {
                        numRef3 = null;
                    }
                    else
                    {
                        numRef3 = (byte*) buffer6[0, 0, 0];
                    }
                    UnsafeNativeMethods.memcpy((void*) numRef3, ptr, size);
                    fixed (byte* numRef3 = null)
                    {
                        return buffer5;
                    }
                }
            }
            throw new ArgumentException();
        }

        private static unsafe bool PtrToBoolean(void* ptr, ref int size)
        {
            if (ptr == null)
            {
                size = 1;
                return false;
            }
            return *(((bool*) ptr));
        }

        private static unsafe short PtrToInt16(void* ptr, ref int size)
        {
            if (ptr == null)
            {
                size = 2;
                return 0;
            }
            if (size < 2)
            {
                throw new ArgumentException();
            }
            return *(((short*) ptr));
        }

        private static unsafe int PtrToInt32(void* ptr, ref int size)
        {
            if (ptr == null)
            {
                size = 4;
                return 0;
            }
            if (size < 4)
            {
                throw new ArgumentException();
            }
            return *(((int*) ptr));
        }

        private static unsafe long PtrToInt64(void* ptr, ref int size)
        {
            if (ptr == null)
            {
                size = 8;
                return 0L;
            }
            if (size < 8)
            {
                throw new ArgumentException();
            }
            return *(((long*) ptr));
        }

        private static unsafe sbyte PtrToInt8(void* ptr, ref int size)
        {
            if (ptr == null)
            {
                size = 1;
                return 0;
            }
            if (size < 1)
            {
                throw new ArgumentException();
            }
            return *(((sbyte*) ptr));
        }

        private static unsafe object PtrToObject(void* ptr, Type type, ref int size)
        {
            if (!type.IsPrimitive)
            {
                if (type == typeof(DT))
                {
                    return new DT(PtrToUInt32(ptr, ref size));
                }
                if (type == typeof(DATE))
                {
                    return new DATE(PtrToUInt32(ptr, ref size));
                }
                if (type == typeof(TIME))
                {
                    return new TIME(PtrToUInt32(ptr, ref size));
                }
                if (type == typeof(LTIME))
                {
                    return new LTIME(PtrToUInt64(ptr, ref size));
                }
                if (type == typeof(TOD))
                {
                    return new TOD(PtrToUInt32(ptr, ref size));
                }
                if (type == typeof(string))
                {
                    throw new ArgumentException($"Unable to marshal type '{type}!", "type");
                }
                if (type.IsArray)
                {
                    throw new ArgumentException($"Unable to marshal type '{type}!", "type");
                }
                return PtrToStruct(ptr, type, ref size);
            }
            if (type == typeof(bool))
            {
                return PtrToBoolean(ptr, ref size);
            }
            if (type == typeof(int))
            {
                return PtrToInt32(ptr, ref size);
            }
            if (type == typeof(short))
            {
                return PtrToInt16(ptr, ref size);
            }
            if (type == typeof(byte))
            {
                return PtrToUInt8(ptr, ref size);
            }
            if (type == typeof(float))
            {
                return PtrToReal32(ptr, ref size);
            }
            if (type == typeof(double))
            {
                return PtrToReal64(ptr, ref size);
            }
            if (type == typeof(long))
            {
                return PtrToInt64(ptr, ref size);
            }
            if (type == typeof(ulong))
            {
                return PtrToUInt64(ptr, ref size);
            }
            if (type == typeof(uint))
            {
                return PtrToUInt32(ptr, ref size);
            }
            if (type == typeof(ushort))
            {
                return PtrToUInt16(ptr, ref size);
            }
            if (type != typeof(sbyte))
            {
                throw new ArgumentException($"Cannot marshal type '{type}!", "type");
            }
            return PtrToInt8(ptr, ref size);
        }

        public static unsafe object PtrToObject(void* ptr, int size, Type type, int[] args) => 
            PtrToObject(ptr, type, args, ref size);

        private static unsafe object PtrToObject(void* ptr, Type type, int[] args, ref int size)
        {
            if (args == null)
            {
                return PtrToObject(ptr, type, ref size);
            }
            if (!type.IsArray)
            {
                if (type == typeof(string))
                {
                    return PtrToString(ptr, args[0], ref size);
                }
            }
            else
            {
                Type elementType = type.GetElementType();
                if (elementType.IsPrimitive)
                {
                    if (type.GetArrayRank() != args.Rank)
                    {
                        throw new ArgumentException("Inconsistent array ranks!", "args");
                    }
                    if (elementType == typeof(bool))
                    {
                        return PtrToArrayOfBoolean(ptr, args, ref size);
                    }
                    if (elementType == typeof(int))
                    {
                        return PtrToArrayOfInt32(ptr, args, ref size);
                    }
                    if (elementType == typeof(short))
                    {
                        return PtrToArrayOfInt16(ptr, args, ref size);
                    }
                    if (elementType == typeof(byte))
                    {
                        return PtrToArrayOfUInt8(ptr, args, ref size);
                    }
                    if (elementType == typeof(float))
                    {
                        return PtrToArrayOfReal32(ptr, args, ref size);
                    }
                    if (elementType == typeof(double))
                    {
                        return PtrToArrayOfReal64(ptr, args, ref size);
                    }
                    if (elementType == typeof(long))
                    {
                        return PtrToArrayOfInt64(ptr, args, ref size);
                    }
                    if (elementType == typeof(uint))
                    {
                        return PtrToArrayOfUInt32(ptr, args, ref size);
                    }
                    if (elementType == typeof(ulong))
                    {
                        return PtrToArrayOfUInt64(ptr, args, ref size);
                    }
                    if (elementType == typeof(ushort))
                    {
                        return PtrToArrayOfUInt16(ptr, args, ref size);
                    }
                    if (elementType != typeof(sbyte))
                    {
                        throw new ArgumentException($"Unable to marshal type '{type}!", "type");
                    }
                    return PtrToArrayOfInt8(ptr, args, ref size);
                }
                if (type == typeof(string[]))
                {
                    return PtrToArrayOfString(ptr, args[0], args[1], ref size);
                }
                if (!elementType.IsArray && (type.GetArrayRank() == 1))
                {
                    return PtrToArrayOfStruct(ptr, elementType, args[0], ref size);
                }
            }
            throw new ArgumentException($"Unable to marshal type '{type}!", "type");
        }

        private static unsafe float PtrToReal32(void* ptr, ref int size)
        {
            if (ptr == null)
            {
                size = 4;
                return 0f;
            }
            if (size < 4)
            {
                throw new ArgumentException();
            }
            return *(((float*) ptr));
        }

        private static unsafe double PtrToReal64(void* ptr, ref int size)
        {
            if (ptr == null)
            {
                size = 8;
                return 0.0;
            }
            if (size < 8)
            {
                throw new ArgumentException();
            }
            return *(((double*) ptr));
        }

        internal static string PtrToString(IntPtr ptr, int len, Encoding encoding)
        {
            if (ReferenceEquals(encoding, Encoding.Default))
            {
                return Marshal.PtrToStringAnsi(ptr, len);
            }
            if (!ReferenceEquals(encoding, Encoding.Unicode))
            {
                throw new ArgumentOutOfRangeException("encoding");
            }
            return Marshal.PtrToStringUni(ptr, len);
        }

        private static unsafe string PtrToString(void* ptr, int characters, ref int size)
        {
            int len = characters + 1;
            if (ptr == null)
            {
                size = len;
                return null;
            }
            if (len < size)
            {
                throw new ArgumentException();
            }
            size = len;
            string str = PtrToStringAnsi(new IntPtr(ptr), len);
            int index = str.IndexOf('\0');
            if (index != -1)
            {
                str = str.Substring(0, index);
            }
            return str;
        }

        public static string PtrToStringAnsi(IntPtr ptr, int len) => 
            PtrToString(ptr, len, Encoding.Default);

        public static string PtrToStringUnicode(IntPtr ptr, int len) => 
            PtrToString(ptr, len, Encoding.Unicode);

        private static unsafe object PtrToStruct(void* ptr, Type structureType, ref int size)
        {
            int num = Marshal.SizeOf(structureType);
            if (ptr == null)
            {
                size = num;
                return null;
            }
            if (num < size)
            {
                throw new ArgumentException();
            }
            size = num;
            return Marshal.PtrToStructure(new IntPtr(ptr), structureType);
        }

        private static unsafe ushort PtrToUInt16(void* ptr, ref int size)
        {
            if (ptr == null)
            {
                size = 2;
                return 0;
            }
            if (size < 2)
            {
                throw new ArgumentException();
            }
            return *(((ushort*) ptr));
        }

        private static unsafe uint PtrToUInt32(void* ptr, ref int size)
        {
            if (ptr == null)
            {
                size = 4;
                return 0;
            }
            if (size < 4)
            {
                throw new ArgumentException();
            }
            return *(((uint*) ptr));
        }

        private static unsafe ulong PtrToUInt64(void* ptr, ref int size)
        {
            if (ptr == null)
            {
                size = 8;
                return 0UL;
            }
            if (size < 8)
            {
                throw new ArgumentException();
            }
            return *(((ulong*) ptr));
        }

        private static unsafe byte PtrToUInt8(void* ptr, ref int size)
        {
            if (ptr == null)
            {
                size = 1;
                return 0;
            }
            if (size < 1)
            {
                throw new ArgumentException();
            }
            return *(((byte*) ptr));
        }

        public static int SizeOf(Type type)
        {
            int size = 0;
            PtrToObject(null, type, ref size);
            return size;
        }

        public static int SizeOf(Type type, int[] args)
        {
            int size = 0;
            PtrToObject(null, type, args, ref size);
            return size;
        }

        internal static IntPtr StringToPtr(string s, Encoding encoding)
        {
            if (ReferenceEquals(encoding, Encoding.Default))
            {
                return Marshal.StringToHGlobalAnsi(s);
            }
            if (!ReferenceEquals(encoding, Encoding.Unicode))
            {
                throw new ArgumentOutOfRangeException("encoding");
            }
            return Marshal.StringToHGlobalUni(s);
        }

        public static IntPtr StringToPtrAnsi(string s) => 
            StringToPtr(s, Encoding.Default);

        public static IntPtr StringToPtrUnicode(string s) => 
            StringToPtr(s, Encoding.Unicode);

        private class UnsafeNativeMethods
        {
            [DllImport("kernel32.dll", EntryPoint="RtlMoveMemory")]
            internal static extern unsafe ushort memcpy(void* dest, void* src, int size);
        }
    }
}

