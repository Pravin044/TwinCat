namespace TwinCAT.Ads
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface IAdsReadWriteTimeoutAccess
    {
        int Read(uint indexGroup, uint indexOffset, AdsStream dataStream, int offset, int length, int timeout);
        int Read(uint indexGroup, uint indexOffset, byte[] readBuffer, int offset, int length, int timeout);
        object ReadAny(uint indexGroup, uint indexOffset, Type type, int[] args, int timeout);
        int ReadWrite(uint indexGroup, uint indexOffset, AdsStream rdDataStream, AdsStream wrDataStream, int timeout);
        int ReadWrite(uint indexGroup, uint indexOffset, AdsStream rdDataStream, int rdOffset, int rdLength, AdsStream wrDataStream, int wrOffset, int wrLength, int timeout);
        int ReadWrite(uint indexGroup, uint indexOffset, byte[] readBuffer, int rdOffset, int rdLength, byte[] writeBuffer, int wrOffset, int wrLength, int timeout);
        AdsErrorCode TryRead(uint indexGroup, uint indexOffset, AdsStream dataStream, int offset, int length, int timeout, out int readBytes);
        AdsErrorCode TryRead(uint indexGroup, uint indexOffset, byte[] readBuffer, int offset, int length, int timeout, out int readBytes);
        AdsErrorCode TryReadWrite(uint indexGroup, uint indexOffset, AdsStream rdDataStream, int rdOffset, int rdLength, AdsStream wrDataStream, int wrOffset, int wrLength, int timeout, out int readBytes);
        AdsErrorCode TryReadWrite(uint indexGroup, uint indexOffset, byte[] readBuffer, int rdOffset, int rdLength, byte[] writeBuffer, int wrOffset, int wrLength, int timeout, out int readBytes);
        AdsErrorCode TryWrite(uint indexGroup, uint indexOffset, AdsStream dataStream, int offset, int length, int timeout);
        AdsErrorCode TryWrite(uint indexGroup, uint indexOffset, byte[] writeBuffer, int offset, int length, int timeout);
        void Write(uint indexGroup, uint indexOffset, int timeout);
        void Write(uint indexGroup, uint indexOffset, AdsStream dataStream, int timeout);
        void Write(uint indexGroup, uint indexOffset, AdsStream dataStream, int offset, int length, int timeout);
        void Write(uint indexGroup, uint indexOffset, byte[] writeBuffer, int offset, int length, int timeout);
        void WriteAny(uint indexGroup, uint indexOffset, object value, int[] args, int timeout);
    }
}

