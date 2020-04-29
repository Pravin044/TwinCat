namespace TwinCAT.Ads
{
    using System;

    public sealed class AmsRouterNotificationEventArgs : EventArgs
    {
        private AmsRouterState state;

        public AmsRouterNotificationEventArgs(AmsRouterState state)
        {
            this.state = state;
        }

        public AmsRouterState State =>
            this.state;
    }
}

