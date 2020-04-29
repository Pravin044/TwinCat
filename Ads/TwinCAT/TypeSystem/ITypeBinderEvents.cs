namespace TwinCAT.TypeSystem
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface ITypeBinderEvents
    {
        event EventHandler<DataTypeNameEventArgs> TypeResolveError;

        event EventHandler<DataTypeEventArgs> TypesGenerated;
    }
}

