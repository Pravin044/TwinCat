namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    internal class AdsSysServState
    {
        internal AdsState adsState;
        internal ushort deviceState;
        internal ushort restartIndex;
        internal byte version;
        internal byte revision;
        internal ushort build;
        internal byte platform;
        internal byte osType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=2)]
        internal ushort[] reserved = new ushort[2];
    }
}

