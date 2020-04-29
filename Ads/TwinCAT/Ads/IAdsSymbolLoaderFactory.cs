namespace TwinCAT.Ads
{
    using System.ComponentModel;
    using TwinCAT;
    using TwinCAT.Ads.TypeSystem;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface IAdsSymbolLoaderFactory
    {
        IAdsSymbolLoader CreateSymbolLoader(ISession session, SymbolLoaderSettings settings);
    }
}

