namespace TwinCAT.TypeSystem
{
    using TwinCAT;

    public interface IValueSymbol2 : IValueSymbol, IValueRawSymbol, IHierarchicalSymbol, ISymbol, IAttributedInstance, IInstance, IBitSize
    {
        IConnection Connection { get; }
    }
}

