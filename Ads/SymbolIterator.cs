using System;
using System.Collections.Generic;
using TwinCAT.TypeSystem.Generic;

public class SymbolIterator : SymbolIterator<ISymbol>
{
    public SymbolIterator(IInstanceCollection<ISymbol> coll) : this(coll, null)
    {
    }

    public SymbolIterator(IEnumerable<ISymbol> coll, bool recurse) : this(coll, recurse, null)
    {
    }

    public SymbolIterator(IInstanceCollection<ISymbol> coll, Func<ISymbol, bool> predicate) : base(coll, predicate)
    {
    }

    public SymbolIterator(IEnumerable<ISymbol> coll, bool recurse, Func<ISymbol, bool> predicate) : base(coll, recurse, predicate)
    {
    }
}

