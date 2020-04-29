namespace TwinCAT.Ads
{
    using System;
    using System.Runtime.InteropServices;

    public interface IAdsHandleAccess
    {
        int CreateVariableHandle(string variableName);
        void DeleteVariableHandle(int variableHandle);
        int Read(int variableHandle, AdsStream dataStream);
        int Read(int variableHandle, AdsStream dataStream, int offset, int length);
        int ReadWrite(int variableHandle, AdsStream rdDataStream, int rdOffset, int rdLength, AdsStream wrDataStream, int wrOffset, int wrLength);
        AdsErrorCode TryRead(int variableHandle, AdsStream dataStream, int offset, int length, out int readBytes);
        AdsErrorCode TryReadWrite(int variableHandle, AdsStream rdDataStream, int rdOffset, int rdLength, AdsStream wrDataStream, int wrOffset, int wrLength, out int readBytes);
        AdsErrorCode TryWrite(int variableHandle, AdsStream dataStream, int offset, int length);
        void Write(int variableHandle, AdsStream dataStream);
        void Write(int variableHandle, AdsStream dataStream, int offset, int length);
    }
}

