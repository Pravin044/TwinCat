namespace TwinCAT.TypeSystem
{
    public interface IOversamplingArrayInstance : IArrayInstance, ISymbol, IAttributedInstance, IInstance, IBitSize
    {
        ISymbol OversamplingElement { get; }
    }
}

