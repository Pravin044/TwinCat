namespace TwinCAT.TypeSystem
{
    using System.ComponentModel;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface IBinderProvider
    {
        IBinder Binder { get; }
    }
}

