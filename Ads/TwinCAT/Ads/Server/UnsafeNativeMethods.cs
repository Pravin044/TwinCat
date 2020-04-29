namespace TwinCAT.Ads.Server
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal class UnsafeNativeMethods
    {
        [DllImport("TcAmsServer.dll")]
        internal static extern ushort AmsConnect(ushort port, [MarshalAs(UnmanagedType.LPWStr)] string portName, AmsServerReceiveDelegate OnAmsServerReceive);
        [DllImport("TcAmsServer.dll")]
        internal static extern uint AmsDisconnect(ushort port);
        [DllImport("TcAmsServer.dll")]
        internal static extern unsafe uint AmsSend(ushort port, void* pAmsCmd);
        [DllImport("TcAmsServer.dll")]
        internal static extern void AmsServerAPICleanup();
        [DllImport("TcAmsServer.dll")]
        internal static extern void AmsServerAPIStartup();
        [DllImport("TcAmsServer.dll")]
        internal static extern unsafe uint GetServerAddress(ushort port, void* pServerAddress);

        internal delegate void AmsServerReceiveDelegate(IntPtr pAmsCmd);
    }
}

