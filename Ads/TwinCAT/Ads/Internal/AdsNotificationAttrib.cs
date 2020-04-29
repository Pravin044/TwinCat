namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct AdsNotificationAttrib
    {
        public int cbLength;
        public int nTransMode;
        public int nMaxDelay;
        public int nCycleTime;
    }
}

