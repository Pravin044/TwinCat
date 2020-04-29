namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using TwinCAT.TypeSystem.Generic;

    public class ReadOnlySymbolCollection : ReadOnlySymbolCollection<ISymbol>, ISymbolCollection, ISymbolCollection<ISymbol>, IInstanceCollection<ISymbol>, IList<ISymbol>, ICollection<ISymbol>, IEnumerable<ISymbol>, IEnumerable
    {
        public ReadOnlySymbolCollection(IInstanceCollection<ISymbol> symbols) : base(symbols)
        {
        }
    }
}

