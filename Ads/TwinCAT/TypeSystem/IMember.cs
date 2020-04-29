namespace TwinCAT.TypeSystem
{
    using System;

    public interface IMember : IField, IAttributedInstance, IInstance, IBitSize
    {
        int Offset { get; }

        int ByteOffset { get; }

        int BitOffset { get; }
    }
}

