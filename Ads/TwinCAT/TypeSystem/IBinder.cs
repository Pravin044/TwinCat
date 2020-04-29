namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface IBinder : IDataTypeResolver
    {
        ISymbol Bind(IHierarchicalSymbol subSymbol);
        void OnTypeGenerated(IDataType type);
        void OnTypeResolveError(string typeName);
        void OnTypesGenerated(IEnumerable<IDataType> types);
        void RegisterType(IDataType type);
        void RegisterTypes(IEnumerable<IDataType> types);

        IInternalSymbolProvider Provider { get; }
    }
}

