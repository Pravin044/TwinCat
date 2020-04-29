namespace TwinCAT.TypeSystem
{
    using System;
    using System.Runtime.InteropServices;

    public interface IValue
    {
        void Read();
        object ResolveValue(bool resolveEnumToPrimitive);
        bool TryResolveValue(bool resolveEnumToPrimitive, out object value);
        void Write();

        ISymbol Symbol { get; }

        IDataType DataType { get; }

        ValueUpdateMode UpdateMode { get; }

        byte[] CachedRaw { get; }

        DateTime UtcTimeStamp { get; }

        TimeSpan Age { get; }

        bool IsPrimitive { get; }
    }
}

