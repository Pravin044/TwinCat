namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Text;
    using TwinCAT.Ads;

    internal interface IAdsEnumCustomMarshal<T>
    {
        void Read(uint enumValueSize, long parentEndPosition, Encoding encoding, AdsBinaryReader reader);
    }
}

