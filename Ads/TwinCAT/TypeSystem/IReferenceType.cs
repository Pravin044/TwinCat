namespace TwinCAT.TypeSystem
{
    using System;

    public interface IReferenceType : IDataType, IBitSize
    {
        IDataType ReferencedType { get; }

        DataTypeCategory ResolvedCategory { get; }

        int ResolvedByteSize { get; }

        IDataType ResolvedType { get; }
    }
}

