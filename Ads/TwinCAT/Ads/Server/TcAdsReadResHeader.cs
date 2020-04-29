namespace TwinCAT.Ads.Server
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    internal class TcAdsReadResHeader : ITcAdsHeader
    {
        internal AdsErrorCode _result;
        internal uint _cbData;
        public void WriteToBuffer(BinaryWriter writer)
        {
            writer.Write((uint) this._result);
            writer.Write(this._cbData);
        }
    }
}

