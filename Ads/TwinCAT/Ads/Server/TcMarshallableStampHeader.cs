namespace TwinCAT.Ads.Server
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    internal class TcMarshallableStampHeader
    {
        internal long timeStamp;
        internal uint numSamples;
    }
}

