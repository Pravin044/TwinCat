namespace TwinCAT.Ads.Internal
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;

    [Browsable(false), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface ITcAdsRaw : ITcAdsRawPrimitives, ITcAdsRawAny
    {
        AdsErrorCode AmsPortEnabled(bool throwAdsException, out bool enabled);
        AdsErrorCode Read(int variableHandle, int offset, int length, byte[] data, bool throwAdsException, out int dataRead);
        unsafe AdsErrorCode Read(uint indexGroup, uint indexOffset, int length, void* data, bool throwAdsException, out int dataRead);
        AdsErrorCode Read(uint indexGroup, uint indexOffset, int offset, int length, byte[] data, bool throwAdsException, out int dataRead);
        AdsErrorCode ReadState(bool throwAdsException, out StateInfo stateInfo);
        AdsErrorCode ReadWrite(uint indexGroup, uint indexOffset, string wrValue, bool bThrowAdsException, out uint rdValue);
        unsafe AdsErrorCode ReadWrite(uint indexGroup, uint indexOffset, int readLength, void* readData, int writeLength, void* writeData, bool throwAdsException, out int dataRead);
        AdsErrorCode ReadWrite(int variableHandle, int rdOffset, int rdLength, byte[] rdData, int wrOffset, int wrLength, byte[] wrData, bool throwAdsException, out int dataRead);
        AdsErrorCode ReadWrite(uint indexGroup, uint indexOffset, int rdOffset, int rdLength, byte[] rdData, int wrOffset, int wrLength, byte[] wrData, bool throwAdsException, out int dataRead);
        AdsErrorCode TryCreateVariableHandle(string variableName, bool throwAdsException, out int handle);
        AdsErrorCode TryDeleteVariableHandle(int variableHandle, bool throwAdsException);
        AdsErrorCode Write(int variableHandle, int offset, int length, byte[] data, bool throwAdsException);
        unsafe AdsErrorCode Write(uint indexGroup, uint indexOffset, int length, void* data, bool throwAdsException);
        AdsErrorCode Write(uint indexGroup, uint indexOffset, int offset, int length, byte[] data, bool throwAdsException);
        AdsErrorCode WriteControl(StateInfo stateInfo, byte[] data, int offset, int length, bool throwAdsException);
    }
}

