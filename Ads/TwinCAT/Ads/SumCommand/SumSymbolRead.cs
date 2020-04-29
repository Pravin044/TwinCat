namespace TwinCAT.Ads.SumCommand
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;
    using TwinCAT.Ads.TypeSystem;
    using TwinCAT.TypeSystem;
    using TwinCAT.ValueAccess;

    public class SumSymbolRead : SumSymbolCommand<SumRead>
    {
        public SumSymbolRead(IAdsConnection connection, IList<ISymbol> symbols) : base(connection, symbols)
        {
        }

        protected override IList<SumDataEntity> CreateSumEntityInfos()
        {
            List<SumDataEntity> list = new List<SumDataEntity>();
            foreach (Symbol symbol in base.UnwrappedSymbols)
            {
                IgIoSumEntity item = new IgIoSumEntity(symbol.IndexGroup, symbol.IndexOffset, symbol.ByteSize, 0);
                list.Add(item);
            }
            return list;
        }

        public object[] Read()
        {
            AdsErrorCode[] returnCodes = null;
            object[] values = null;
            AdsErrorCode code = this.TryRead(out values, out returnCodes);
            if (base.Failed)
            {
                throw new AdsSumCommandException("SumSymbolRead failed!", this);
            }
            return values;
        }

        public AdsErrorCode TryRead(out object[] values, out AdsErrorCode[] returnCodes)
        {
            IList<SumDataEntity> sumEntities = this.CreateSumEntityInfos();
            base.innerCommand = new SumRead(base.connection, sumEntities, TwinCAT.Ads.SumCommand.SumCommand.SumAccessMode.IndexGroupIndexOffset);
            values = null;
            IList<byte[]> readData = null;
            AdsErrorCode code = base.innerCommand.TryReadRaw(out readData, out returnCodes);
            if (code == AdsErrorCode.NoError)
            {
                values = new object[sumEntities.Count];
                IList<Symbol> unwrappedSymbols = base.UnwrappedSymbols;
                ValueAccessor valueAccessor = base.ValueAccessor;
                for (int i = 0; i < base.symbols.Count; i++)
                {
                    ISymbol symbol = base.symbols[i];
                    if (returnCodes[i] == AdsErrorCode.NoError)
                    {
                        values[i] = valueAccessor.ValueFactory.CreateValue(symbol, readData[i], 0, DateTime.UtcNow);
                    }
                }
            }
            return code;
        }
    }
}

