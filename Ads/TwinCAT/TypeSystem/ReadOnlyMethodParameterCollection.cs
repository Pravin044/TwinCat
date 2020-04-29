namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.ObjectModel;

    public class ReadOnlyMethodParameterCollection : ReadOnlyCollection<IRpcMethodParameter>
    {
        internal ReadOnlyMethodParameterCollection(RpcMethodParameterCollection coll) : base(coll)
        {
        }
    }
}

