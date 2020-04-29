namespace TwinCAT.ValueAccess
{
    using System.ComponentModel;
    using TwinCAT;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface IAccessorConnection
    {
        IConnection Connection { get; }
    }
}

