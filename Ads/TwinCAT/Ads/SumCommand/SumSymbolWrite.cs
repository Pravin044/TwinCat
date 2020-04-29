namespace TwinCAT.Ads.SumCommand
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;
    using TwinCAT.Ads.TypeSystem;
    using TwinCAT.TypeSystem;

    public class SumSymbolWrite : SumSymbolCommand<SumWrite>
    {
        public SumSymbolWrite(IAdsConnection connection, IList<ISymbol> symbols) : base(connection, symbols)
        {
        }

        protected override IList<SumDataEntity> CreateSumEntityInfos()
        {
            List<SumDataEntity> list = new List<SumDataEntity>();
            foreach (Symbol symbol in base.UnwrappedSymbols)
            {
                IgIoSumEntity item = new IgIoSumEntity(symbol.IndexGroup, symbol.IndexOffset, 0, symbol.ByteSize);
                list.Add(item);
            }
            return list;
        }

        public AdsErrorCode TryWrite(object[] values, out AdsErrorCode[] returnCodes)
        {
            IList<SumDataEntity> sumEntities = this.CreateSumEntityInfos();
            base.innerCommand = new SumWrite(base.connection, sumEntities, TwinCAT.Ads.SumCommand.SumCommand.SumAccessMode.IndexGroupIndexOffset);
            List<byte[]> writeData = new List<byte[]>();
            IList<Symbol> unwrappedSymbols = base.UnwrappedSymbols;
            InstanceValueConverter converter = new InstanceValueConverter();
            for (int i = 0; i < unwrappedSymbols.Count; i++)
            {
                SumDataEntity entity = sumEntities[i];
                Symbol symbol = unwrappedSymbols[i];
                byte[] item = converter.Marshal(symbol, values[i]);
                if ((symbol.Category == DataTypeCategory.String) && (item.Length < entity.WriteLength))
                {
                    entity.SetWriteLength(item.Length);
                }
                writeData.Add(item);
            }
            return base.innerCommand.TryWriteRaw(writeData, out returnCodes);
        }

        public void Write(object[] values)
        {
            AdsErrorCode[] returnCodes = null;
            AdsErrorCode code = this.TryWrite(values, out returnCodes);
            if (base.Failed)
            {
                throw new AdsSumCommandException("SumSymbolWrite failed!", this);
            }
        }
    }
}

