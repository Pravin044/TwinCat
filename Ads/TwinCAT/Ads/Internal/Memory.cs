namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Runtime.InteropServices;

    internal class Memory
    {
        private static IntPtr ph = NativeMethods.GetProcessHeap();
        private const int HEAP_ZERO_MEMORY = 8;

        private Memory()
        {
        }

        public static unsafe void* Alloc(int size)
        {
            void* voidPtr = NativeMethods.HeapAlloc(ph, 8, size);
            if (voidPtr == null)
            {
                throw new OutOfMemoryException();
            }
            return voidPtr;
        }

        public static unsafe void Copy(void* src, void* dst, int count)
        {
            byte* numPtr = (byte*) src;
            byte* numPtr2 = (byte*) dst;
            if (numPtr > numPtr2)
            {
                while (count != 0)
                {
                    numPtr2++;
                    numPtr++;
                    numPtr2[0] = numPtr[0];
                    count--;
                }
            }
            else if (numPtr < numPtr2)
            {
                numPtr += count;
                numPtr2 += count;
                while (count != 0)
                {
                    *(--numPtr2) = *(--numPtr);
                    count--;
                }
            }
        }

        public static unsafe void Free(void* block)
        {
            if (!NativeMethods.HeapFree(ph, 0, block))
            {
                throw new InvalidOperationException();
            }
        }

        public static unsafe bool MemCmp(void* src1, void* src2, int count)
        {
            byte* numPtr = (byte*) src1;
            byte* numPtr2 = (byte*) src2;
            while (count != 0)
            {
                numPtr2++;
                numPtr++;
                if (numPtr2[0] != numPtr[0])
                {
                    return false;
                }
                count--;
            }
            return true;
        }

        public static unsafe void* ReAlloc(void* block, int size)
        {
            void* voidPtr = NativeMethods.HeapReAlloc(ph, 8, block, size);
            if (voidPtr == null)
            {
                throw new OutOfMemoryException();
            }
            return voidPtr;
        }

        public static unsafe int SizeOf(void* block)
        {
            int num = NativeMethods.HeapSize(ph, 0, block);
            if (num == -1)
            {
                throw new InvalidOperationException();
            }
            return num;
        }

        private class NativeMethods
        {
            [DllImport("kernel32")]
            internal static extern IntPtr GetProcessHeap();
            [DllImport("kernel32")]
            internal static extern unsafe void* HeapAlloc(IntPtr hHeap, int flags, int size);
            [DllImport("kernel32")]
            internal static extern unsafe bool HeapFree(IntPtr hHeap, int flags, void* block);
            [DllImport("kernel32")]
            internal static extern unsafe void* HeapReAlloc(IntPtr hHeap, int flags, void* block, int size);
            [DllImport("kernel32")]
            internal static extern unsafe int HeapSize(IntPtr hHeap, int flags, void* block);
        }
    }
}

