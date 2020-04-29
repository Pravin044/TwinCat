namespace TwinCAT.Ads.Internal
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;

    [Browsable(false), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface ITcAdsRawAny
    {
        AdsErrorCode ReadAny(int variableHandle, Type type, bool throwAdsException, out object value);
        AdsErrorCode ReadAny(int variableHandle, Type type, int[] args, bool throwAdsException, out object value);
        AdsErrorCode ReadAny(uint indexGroup, uint indexOffset, Type type, bool throwAdsException, out object value);
        AdsErrorCode ReadAny(uint indexGroup, uint indexOffset, Type type, int[] args, bool throwAdsException, out object value);
        object ReadStruct(uint indexGroup, uint indexOffset, Type structureType, bool throwAdsException, out AdsErrorCode result);
        AdsErrorCode Write(uint indexGroup, uint indexOffset, bool val, bool throwAdsException);
        AdsErrorCode Write(uint indexGroup, uint indexOffset, byte val, bool throwAdsException);
        AdsErrorCode Write(uint indexGroup, uint indexOffset, double val, bool throwAdsException);
        AdsErrorCode Write(uint indexGroup, uint indexOffset, short val, bool throwAdsException);
        AdsErrorCode Write(uint indexGroup, uint indexOffset, int val, bool throwAdsException);
        AdsErrorCode Write(uint indexGroup, uint indexOffset, long val, bool throwAdsException);
        AdsErrorCode Write(uint indexGroup, uint indexOffset, object structure, bool throwAdsException);
        AdsErrorCode Write(uint indexGroup, uint indexOffset, sbyte val, bool throwAdsException);
        AdsErrorCode Write(uint indexGroup, uint indexOffset, float val, bool throwAdsException);
        AdsErrorCode Write(uint indexGroup, uint indexOffset, ushort val, bool throwAdsException);
        AdsErrorCode Write(uint indexGroup, uint indexOffset, uint val, bool throwAdsException);
        AdsErrorCode Write(uint indexGroup, uint indexOffset, string val, int characters, bool throwAdsException);
        AdsErrorCode WriteAny(int variableHandle, object value, bool throwAdsException);
        AdsErrorCode WriteAny(int variableHandle, object value, int[] args, bool throwAdsException);
        AdsErrorCode WriteAny(uint indexGroup, uint indexOffset, object value, bool throwAdsException);
        AdsErrorCode WriteAny(uint indexGroup, uint indexOffset, object value, int[] args, bool throwAdsException);
    }
}

