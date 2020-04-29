namespace TwinCAT.Ads.SumCommand
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using TwinCAT.Ads;
    using TwinCAT.Ads.TypeSystem;
    using TwinCAT.Ads.ValueAccess;
    using TwinCAT.TypeSystem;
    using TwinCAT.ValueAccess;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public abstract class SumSymbolCommand<T> : SumCommandWrapper<T> where T: ISumCommand
    {
        protected IAdsConnection connection;
        protected ValueAccessMode mode;
        protected IList<ISymbol> symbols;

        protected SumSymbolCommand(IAdsConnection connection, IList<ISymbol> symbols)
        {
            this.mode = ValueAccessMode.IndexGroupOffsetPreferred;
            this.connection = connection;
            this.symbols = symbols;
        }

        protected abstract IList<SumDataEntity> CreateSumEntityInfos();

        protected IList<Symbol> UnwrappedSymbols
        {
            get
            {
                List<Symbol> list = new List<Symbol>();
                for (int i = 0; i < this.symbols.Count; i++)
                {
                    Symbol item = this.symbols[i] as Symbol;
                    if (item == null)
                    {
                        item = (Symbol) ((IDynamicSymbol) this.symbols[i]).Unwrap();
                    }
                    list.Add(item);
                }
                return list;
            }
        }

        protected TwinCAT.ValueAccess.ValueAccessor ValueAccessor
        {
            get
            {
                IList<Symbol> unwrappedSymbols = this.UnwrappedSymbols;
                if ((unwrappedSymbols == null) || (unwrappedSymbols.Count <= 0))
                {
                    return null;
                }
                return (TwinCAT.ValueAccess.ValueAccessor) unwrappedSymbols[0].ValueAccessor;
            }
        }
    }
}

