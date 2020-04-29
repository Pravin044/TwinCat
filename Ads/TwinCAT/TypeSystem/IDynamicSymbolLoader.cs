namespace TwinCAT.TypeSystem
{
    public interface IDynamicSymbolLoader : ISymbolLoader, ISymbolProvider
    {
        DynamicSymbolsContainer SymbolsDynamic { get; }
    }
}

