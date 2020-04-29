namespace TwinCAT.Ads
{
    using System;

    public class AdsCommunicationStatistics
    {
        private AdsSession _session;

        internal AdsCommunicationStatistics(AdsSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }
            this._session = session;
        }

        public int TotalCycles =>
            ((this._session.ConnectionObserver == null) ? 0 : this._session.ConnectionObserver.TotalCycles);

        public int TotalErrors =>
            ((this._session.ConnectionObserver == null) ? 0 : this._session.ConnectionObserver.TotalErrors);

        public DateTime? LastSucceededAccess
        {
            get
            {
                if (this._session.IsConnected)
                {
                    return new DateTime?(this._session.ConnectionObserver.LastSucceededAccess);
                }
                return null;
            }
        }

        public TimeSpan AccessWaitTime =>
            (!this._session.IsConnected ? TimeSpan.Zero : this._session.Connection.AccessWaitTime);

        public int ErrorsSinceLastSucceeded =>
            ((this._session.ConnectionObserver == null) ? 0 : this._session.ConnectionObserver.ErrorsSinceLastSucceeded);

        public int ConnectionResurrections =>
            (!this._session.IsConnected ? 0 : this._session.Connection.Resurrections);

        public DateTime SessionEstablishedAt =>
            this._session.EstablishedAt;

        public DateTime? ConnectionEstablishedAt
        {
            get
            {
                if (this._session.IsConnected)
                {
                    return this._session.Connection.ConnectionEstablishedAt;
                }
                return null;
            }
        }

        public DateTime? ConnectionActiveSince
        {
            get
            {
                if (this._session.IsConnected)
                {
                    return this._session.Connection.ActiveSince;
                }
                return null;
            }
        }

        public int ConnectionLostCount =>
            (!this._session.IsConnected ? 0 : this._session.Connection.ConnectionLostCount);

        public DateTime? ConnectionLostTime
        {
            get
            {
                if (this._session.IsConnected)
                {
                    return this._session.Connection.ConnectionLostTime;
                }
                return null;
            }
        }

        public int Resurrections =>
            (!this._session.IsConnected ? 0 : this._session.Connection.Resurrections);
    }
}

