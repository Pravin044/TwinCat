namespace TwinCAT
{
    using System;
    using System.ComponentModel;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface ISessionProvider
    {
        ISession Create(object address, ISessionSettings settings);

        string Name { get; }

        SessionProviderCapabilities Capabilities { get; }
    }
}

