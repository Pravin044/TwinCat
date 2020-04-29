namespace TwinCAT.TypeSystem
{
    using TwinCAT;

    public interface ISymbolFactoryServices
    {
        IBinder Binder { get; }

        ISymbolFactory SymbolFactory { get; }

        ISession Session { get; }
    }
}

