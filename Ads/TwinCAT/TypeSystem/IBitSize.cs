namespace TwinCAT.TypeSystem
{
    using System;

    public interface IBitSize
    {
        int Size { get; }

        bool IsBitType { get; }

        int BitSize { get; }

        int ByteSize { get; }

        bool IsByteAligned { get; }
    }
}

