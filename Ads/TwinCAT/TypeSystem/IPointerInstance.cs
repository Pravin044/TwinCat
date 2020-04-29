namespace TwinCAT.TypeSystem
{
    public interface IPointerInstance : ISymbol, IAttributedInstance, IInstance, IBitSize
    {
        ISymbol Reference { get; }
    }
}

