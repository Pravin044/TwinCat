namespace TwinCAT.TypeSystem.Generic
{
    using System;

    public class ReadOnlySymbolCollection<T> : ReadOnlyInstanceCollection<T> where T: ISymbol
    {
        public ReadOnlySymbolCollection(IInstanceCollection<T> coll) : base(coll)
        {
        }
    }
}

