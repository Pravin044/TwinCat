namespace TwinCAT.Ads.Server
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    internal class TcAdsDeviceNotificationReqHeader : ITcAdsHeader
    {
        internal uint _cbData;
        internal uint _numStamps;
        public void WriteToBuffer(BinaryWriter writer)
        {
            writer.Write(this._cbData);
            writer.Write(this._numStamps);
        }
    }
}

