namespace TwinCAT.TypeSystem
{
    using System;

    public interface IReferenceInstance : ISymbol, IAttributedInstance, IInstance, IBitSize
    {
        DataTypeCategory ResolvedCategory { get; }

        int ResolvedByteSize { get; }

        IDataType ReferencedType { get; }

        IDataType ResolvedType { get; }
    }
}

