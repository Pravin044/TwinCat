namespace TwinCAT.Ads.Server
{
    using System;
    using System.IO;

    internal interface ITcAdsHeader
    {
        void WriteToBuffer(BinaryWriter writer);
    }
}

