namespace TwinCAT
{
    using TwinCAT.TypeSystem;

    public interface ISymbolServerProvider
    {
        ISymbolServer SymbolServer { get; }
    }
}

