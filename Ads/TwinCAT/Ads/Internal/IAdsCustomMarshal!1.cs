namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Text;
    using TwinCAT.Ads;

    internal interface IAdsCustomMarshal<T>
    {
        void Read(long parentEndPosition, Encoding encoding, AdsBinaryReader reader);
    }
}

