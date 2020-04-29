namespace TwinCAT.TypeSystem
{
    public interface ISymbolServer
    {
        ReadOnlyDataTypeCollection DataTypes { get; }

        ReadOnlySymbolCollection Symbols { get; }
    }
}

