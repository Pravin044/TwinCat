namespace TwinCAT
{
    using System.ComponentModel;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface ISessionProvider<S, A, C> : ISessionProvider where S: ISession
    {
        S Create(A address, C settings);
    }
}

