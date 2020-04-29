namespace TwinCAT.TypeSystem
{
    public interface IUnionInstance : ISymbol, IAttributedInstance, IInstance, IBitSize
    {
        ReadOnlySymbolCollection FieldInstances { get; }
    }
}

