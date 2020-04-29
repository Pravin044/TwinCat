namespace TwinCAT.Ads
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface IAdsReadWriteAccess
    {
        int Read(uint indexGroup, uint indexOffset, AdsStream dataStream);
        int Read(uint indexGroup, uint indexOffset, AdsStream dataStream, int offset, int length);
        int ReadWrite(uint indexGroup, uint indexOffset, AdsStream rdDataStream, AdsStream wrDataStream);
        int ReadWrite(uint indexGroup, uint indexOffset, AdsStream rdDataStream, int rdOffset, int rdLength, AdsStream wrDataStream, int wrOffset, int wrLength);
        int ReadWrite(uint indexGroup, uint indexOffset, byte[] readBuffer, int rdOffset, int rdLength, byte[] writeBuffer, int wrOffset, int wrLength);
        AdsErrorCode TryRead(uint indexGroup, uint indexOffset, AdsStream dataStream, int offset, int length, out int readBytes);
        AdsErrorCode TryRead(uint indexGroup, uint indexOffset, byte[] buffer, int offset, int length, out int readBytes);
        AdsErrorCode TryReadWrite(uint indexGroup, uint indexOffset, AdsStream rdDataStream, int rdOffset, int rdLength, AdsStream wrDataStream, int wrOffset, int wrLength, out int readBytes);
        AdsErrorCode TryReadWrite(uint indexGroup, uint indexOffset, byte[] readBuffer, int rdOffset, int rdLength, byte[] writeBuffer, int wrOffset, int wrLength, out int readBytes);
        AdsErrorCode TryWrite(uint indexGroup, uint indexOffset, AdsStream dataStream, int offset, int length);
        AdsErrorCode TryWrite(uint indexGroup, uint indexOffset, byte[] buffer, int offset, int length);
        void Write(uint indexGroup, uint indexOffset);
        void Write(uint indexGroup, uint indexOffset, AdsStream dataStream);
        void Write(uint indexGroup, uint indexOffset, AdsStream dataStream, int offset, int length);
        void Write(uint indexGroup, uint indexOffset, byte[] buffer, int offset, int length);
    }
}

