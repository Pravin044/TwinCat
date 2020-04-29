namespace TwinCAT.TypeSystem
{
    using System;
    using System.ComponentModel;
    using TwinCAT.TypeSystem.Generic;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public class ReadOnlyNamespaceCollection : ReadOnlyNamespaceCollection<INamespace<IDataType>, IDataType>
    {
        public ReadOnlyNamespaceCollection(NamespaceCollection coll) : base(coll)
        {
        }
    }
}

