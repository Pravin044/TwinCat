namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct VariableInfo
    {
        public uint indexGroup;
        public uint indexOffset;
        public int length;
    }
}

