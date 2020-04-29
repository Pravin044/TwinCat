namespace TwinCAT.TypeSystem
{
    using TwinCAT;
    using TwinCAT.TypeSystem.Generic;

    public interface ISymbolLoader : ISymbolProvider
    {
        ReadOnlyDataTypeCollection<IDataType> BuildInTypes { get; }

        ISymbolLoaderSettings Settings { get; }
    }
}

