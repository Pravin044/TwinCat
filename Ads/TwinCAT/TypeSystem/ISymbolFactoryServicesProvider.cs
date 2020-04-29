namespace TwinCAT.TypeSystem
{
    using System.ComponentModel;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface ISymbolFactoryServicesProvider
    {
        ISymbolFactoryServices FactoryServices { get; }
    }
}

