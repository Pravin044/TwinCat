namespace TwinCAT.TypeSystem
{
    using System;
    using System.Runtime.CompilerServices;
    using TwinCAT.ValueAccess;

    public interface IValueRawSymbol : IHierarchicalSymbol, ISymbol, IAttributedInstance, IInstance, IBitSize
    {
        event EventHandler<RawValueChangedArgs> RawValueChanged;

        byte[] ReadRawValue();
        byte[] ReadRawValue(int timeout);
        void WriteRawValue(byte[] value);
        void WriteRawValue(byte[] value, int timeout);

        bool HasValue { get; }

        IAccessorRawValue ValueAccessor { get; }
    }
}

