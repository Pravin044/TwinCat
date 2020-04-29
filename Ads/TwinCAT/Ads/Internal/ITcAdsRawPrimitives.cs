namespace TwinCAT.Ads.Internal
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Text;
    using TwinCAT.Ads;

    [Browsable(false), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface ITcAdsRawPrimitives
    {
        bool ReadBoolean(uint indexGroup, uint indexOffset, bool throwAdsException, out AdsErrorCode result);
        short ReadInt16(uint indexGroup, uint indexOffset, bool throwAdsException, out AdsErrorCode result);
        int ReadInt32(uint indexGroup, uint indexOffset, bool throwAdsException, out AdsErrorCode result);
        long ReadInt64(uint indexGroup, uint indexOffset, bool throwAdsException, out AdsErrorCode result);
        sbyte ReadInt8(uint indexGroup, uint indexOffset, bool throwAdsException, out AdsErrorCode result);
        float ReadReal32(uint indexGroup, uint indexOffset, bool throwAdsException, out AdsErrorCode result);
        double ReadReal64(uint indexGroup, uint indexOffset, bool throwAdsException, out AdsErrorCode result);
        string ReadString(int variableHandle, int characters, Encoding encoding, bool throwAdsException, out AdsErrorCode result);
        string ReadString(uint indexGroup, uint indexOffset, int characters, Encoding encoding, bool throwAdsException, out AdsErrorCode result);
        ushort ReadUInt16(uint indexGroup, uint indexOffset, bool throwAdsException, out AdsErrorCode result);
        uint ReadUInt32(uint indexGroup, uint indexOffset, bool throwAdsException, out AdsErrorCode result);
        ulong ReadUInt64(uint indexGroup, uint indexOffset, bool throwAdsException, out AdsErrorCode result);
        byte ReadUInt8(uint indexGroup, uint indexOffset, bool throwAdsException, out AdsErrorCode result);
        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        AdsErrorCode WriteString(int variableHandle, string str, int characters, Encoding encoding, bool throwAdsException);
        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        AdsErrorCode WriteString(uint indexGroup, uint indexOffset, string str, int characters, Encoding encoding, bool throwAdsException);
    }
}

