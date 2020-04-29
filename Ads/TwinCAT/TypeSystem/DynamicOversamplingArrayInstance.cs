namespace TwinCAT.TypeSystem
{
    using System;

    public sealed class DynamicOversamplingArrayInstance : DynamicArrayInstance, IOversamplingArrayInstance, IArrayInstance, ISymbol, IAttributedInstance, IInstance, IBitSize
    {
        internal DynamicOversamplingArrayInstance(IOversamplingArrayInstance symbol) : base(symbol)
        {
        }

        public ISymbol OversamplingElement =>
            ((IOversamplingArrayInstance) base.symbol).OversamplingElement;
    }
}

