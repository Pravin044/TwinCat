namespace TwinCAT.TypeSystem
{
    using System;

    public interface IValueAnySymbol : ISymbol, IAttributedInstance, IInstance, IBitSize
    {
        object ReadAnyValue(Type managedType);
        object ReadAnyValue(Type managedType, int timeout);
        void UpdateAnyValue(ref object managedObject);
        void UpdateAnyValue(ref object managedObject, int timeout);
        void WriteAnyValue(object managedValue);
        void WriteAnyValue(object managedValue, int timeout);
    }
}

