namespace TwinCAT.Ads.Server
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    internal class TcAdsWriteControlReqHeader : ITcAdsHeader
    {
        internal AdsState _adsState;
        internal ushort _deviceState;
        internal uint _cbLength;
        public void WriteToBuffer(BinaryWriter writer)
        {
            writer.Write((short) this._adsState);
            writer.Write(this._deviceState);
            writer.Write(this._cbLength);
        }
    }
}

