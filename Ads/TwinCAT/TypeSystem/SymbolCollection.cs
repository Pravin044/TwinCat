namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using TwinCAT.TypeSystem.Generic;

    public class SymbolCollection : SymbolCollection<ISymbol>, ISymbolCollection, ISymbolCollection<ISymbol>, IInstanceCollection<ISymbol>, IList<ISymbol>, ICollection<ISymbol>, IEnumerable<ISymbol>, IEnumerable
    {
        public SymbolCollection() : this(InstanceCollectionMode.Path)
        {
        }

        public SymbolCollection(IEnumerable<ISymbol> coll) : base(coll, InstanceCollectionMode.Path)
        {
        }

        public SymbolCollection(InstanceCollectionMode mode) : base(mode)
        {
        }

        public ReadOnlySymbolCollection AsReadOnly() => 
            new ReadOnlySymbolCollection(this);

        public SymbolCollection Clone() => 
            new SymbolCollection(this);
    }
}

