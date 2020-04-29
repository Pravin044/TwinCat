namespace TwinCAT.Ads.Server
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    internal class TcAdsAddDeviceNotificationReqHeader : ITcAdsHeader
    {
        internal uint _indexGroup;
        internal uint _indexOffset;
        internal uint _cbLength;
        internal AdsTransMode _transMode;
        internal uint _maxDelay;
        internal uint _cycleTime;
        public void WriteToBuffer(BinaryWriter writer)
        {
            writer.Write(this._indexGroup);
            writer.Write(this._indexOffset);
            writer.Write(this._cbLength);
            writer.Write((int) this._transMode);
            writer.Write(this._maxDelay);
            writer.Write(this._cycleTime);
        }
    }
}

