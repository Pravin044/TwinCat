namespace TwinCAT
{
    using System;

    public abstract class SessionProvider<S, A, C> : ISessionProvider<S, A, C>, ISessionProvider where S: ISession where C: class
    {
        protected SessionProviderCapabilities capabilities;
        private static ISessionProvider<S, A, C> s_self;

        protected SessionProvider()
        {
            this.capabilities = SessionProviderCapabilities.Mask_All;
            if (SessionProvider<S, A, C>.s_self != null)
            {
                throw new Exception("Session provider already instantiated!");
            }
            SessionProvider<S, A, C>.s_self = this;
        }

        protected SessionProvider(SessionProviderCapabilities cap)
        {
            this.capabilities = SessionProviderCapabilities.Mask_All;
            if (SessionProvider<S, A, C>.s_self != null)
            {
                throw new Exception("Session provider already instantiated!");
            }
            this.capabilities = cap;
            SessionProvider<S, A, C>.s_self = this;
        }

        public virtual ISession Create(object address, ISessionSettings settings)
        {
            C local = (C) settings;
            if (settings == null)
            {
                local = (C) settings;
            }
            return this.Create((A) address, local);
        }

        public abstract S Create(A address, C settings);

        public SessionProviderCapabilities Capabilities =>
            this.capabilities;

        public static ISessionProvider<S, A, C> Self =>
            SessionProvider<S, A, C>.s_self;

        public abstract string Name { get; }
    }
}

