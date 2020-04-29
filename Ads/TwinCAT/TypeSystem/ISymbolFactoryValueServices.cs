namespace TwinCAT.TypeSystem
{
    using System.ComponentModel;
    using TwinCAT.ValueAccess;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface ISymbolFactoryValueServices
    {
        IAccessorRawValue ValueAccessor { get; }
    }
}

