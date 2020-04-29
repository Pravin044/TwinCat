namespace TwinCAT.TypeSystem
{
    using System;
    using System.ComponentModel;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface IBindable
    {
        void Bind(IBinder binder);

        bool IsBound { get; }
    }
}

