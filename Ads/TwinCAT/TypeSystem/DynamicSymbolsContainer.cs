namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using TwinCAT.TypeSystem.Generic;

    public sealed class DynamicSymbolsContainer : DynamicObject, IEnumerable<ISymbol>, IEnumerable
    {
        private SymbolCollection<ISymbol> _symbols;
        private Dictionary<string, ISymbol> _normalizedDict = new Dictionary<string, ISymbol>(StringComparer.OrdinalIgnoreCase);

        public DynamicSymbolsContainer(SymbolCollection<ISymbol> symbols)
        {
            if (symbols == null)
            {
                throw new ArgumentNullException("symbols");
            }
            this._symbols = symbols;
            foreach (DynamicSymbol symbol in symbols)
            {
                this._normalizedDict.Add(symbol.NormalizedName, symbol);
            }
        }

        public override IEnumerable<string> GetDynamicMemberNames() => 
            Enumerable.Union<string>(base.GetDynamicMemberNames(), this._normalizedDict.Keys);

        public IEnumerator<ISymbol> GetEnumerator() => 
            this._symbols.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => 
            this._symbols.GetEnumerator();

        public bool TryGetInstance(string instanceSpecifier, out ISymbol symbol) => 
            this._symbols.TryGetInstance(instanceSpecifier, out symbol);

        internal bool TryGetInstanceHierarchically(string instancePath, out ISymbol symbol) => 
            this._symbols.TryGetInstanceHierarchically(instancePath, out symbol);

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            ISymbol symbol = null;
            if (!this._normalizedDict.TryGetValue(binder.Name, out symbol))
            {
                return base.TryGetMember(binder, ref result);
            }
            result = symbol;
            return true;
        }

        public DynamicSymbol this[string name]
        {
            get
            {
                IList<ISymbol> instances = null;
                if (!this._symbols.TryGetInstanceByName(name, out instances))
                {
                    throw new KeyNotFoundException("Symbol name not found in DynamicSymbols collection!");
                }
                return (DynamicSymbol) instances[0];
            }
        }
    }
}

