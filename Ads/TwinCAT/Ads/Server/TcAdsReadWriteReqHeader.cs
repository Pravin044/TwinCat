namespace TwinCAT.Ads.Server
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    internal class TcAdsReadWriteReqHeader : ITcAdsHeader
    {
        internal uint _indexGroup;
        internal uint _indexOffset;
        internal uint _cbReadLength;
        internal uint _cbWriteLength;
        public void WriteToBuffer(BinaryWriter writer)
        {
            writer.Write(this._indexGroup);
            writer.Write(this._indexOffset);
            writer.Write(this._cbReadLength);
            writer.Write(this._cbWriteLength);
        }
    }
}

