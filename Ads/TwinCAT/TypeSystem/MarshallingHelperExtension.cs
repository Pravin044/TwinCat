namespace TwinCAT.TypeSystem
{
    using System;
    using System.Runtime.CompilerServices;

    internal static class MarshallingHelperExtension
    {
        internal static int GetValueMarshalSize(this IDataType type) => 
            ((type.Category != DataTypeCategory.Reference) ? type.ByteSize : ((IReferenceType) type).ResolvedByteSize);

        internal static int GetValueMarshalSize(this ISymbol symbol) => 
            ((symbol.Category != DataTypeCategory.Reference) ? symbol.ByteSize : ((IReferenceInstance) symbol).ResolvedByteSize);
    }
}

