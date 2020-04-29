namespace TwinCAT.Ads.Server
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    internal class TcMarshallableAmsHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=6)]
        internal byte[] targetNetId = new byte[6];
        internal ushort targetPort;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=6)]
        internal byte[] senderNetId = new byte[6];
        internal ushort senderPort;
        internal ushort cmdId;
        internal ushort stateFlags;
        internal uint cbData;
        internal uint errCode;
        internal uint hUser;
    }
}

