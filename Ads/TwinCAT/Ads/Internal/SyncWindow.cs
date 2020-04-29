namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;
    using TwinCAT.Ads;

    internal class SyncWindow : Control, ISyncWindow, IDisposable
    {
        private ISyncNotificationReceiver syncNoteReceiver;
        private ISyncMessageReceiver syncMsgReceiver;
        private NotificationQueueDelegate notificationQueueDelegate;
        private NotificationDelegate notificationDelegate;
        private RouterNotificationDelegate routerNotificationDelegate;
        private StateChangedDelegate stateChangedDelegate;
        private SymbolVersionChangedDelegate symbolVersionChangedDelegate;

        public SyncWindow(ISyncMessageReceiver syncMsgReceiver)
        {
            this.syncMsgReceiver = syncMsgReceiver;
            base.CreateControl();
            this.notificationDelegate = new NotificationDelegate(this.OnSyncNotification);
            this.routerNotificationDelegate = new RouterNotificationDelegate(this.OnSyncRouterNotification);
        }

        public SyncWindow(ISyncNotificationReceiver syncNoteReceiver)
        {
            this.syncNoteReceiver = syncNoteReceiver;
            base.CreateControl();
            this.notificationQueueDelegate = new NotificationQueueDelegate(this.OnSyncQueueNotification);
        }

        public SyncWindow(ISyncMessageReceiver syncMsgReceiver, ISyncNotificationReceiver syncNoteReceiver)
        {
            this.syncMsgReceiver = syncMsgReceiver;
            this.syncNoteReceiver = syncNoteReceiver;
            base.CreateControl();
            this.notificationDelegate = new NotificationDelegate(this.OnSyncNotification);
            this.notificationQueueDelegate = new NotificationQueueDelegate(this.OnSyncQueueNotification);
            this.routerNotificationDelegate = new RouterNotificationDelegate(this.OnSyncRouterNotification);
            this.stateChangedDelegate = new StateChangedDelegate(this.OnAdsStateChanged);
            this.symbolVersionChangedDelegate = new SymbolVersionChangedDelegate(this.OnSymbolVersionChanged);
        }

        public void OnAdsStateChanged(AdsStateChangedEventArgs eventArgs)
        {
            if (this.syncMsgReceiver != null)
            {
                this.syncMsgReceiver.OnAdsStateChanged(eventArgs);
            }
        }

        public void OnSymbolVersionChanged(AdsSymbolVersionChangedEventArgs eventArgs)
        {
            if (this.syncMsgReceiver != null)
            {
                this.syncMsgReceiver.OnSymbolVersionChanged(eventArgs);
            }
        }

        public void OnSyncNotification(int handle, long timeStamp, int length, NotificationEntry entry, bool bError)
        {
            if (this.syncMsgReceiver != null)
            {
                this.syncMsgReceiver.OnSyncNotification(handle, timeStamp, length, entry, bError);
            }
        }

        public void OnSyncQueueNotification(QueueElement[] elements)
        {
            if (this.syncNoteReceiver != null)
            {
                this.syncNoteReceiver.OnSyncNotification(elements);
            }
        }

        public void OnSyncRouterNotification(AmsRouterState state)
        {
            if (this.syncMsgReceiver != null)
            {
                this.syncMsgReceiver.OnSyncRouterNotification(state);
            }
        }

        public void PostAdsStateChanged(AdsStateChangedEventArgs eventArgs)
        {
            if (this.syncMsgReceiver != null)
            {
                object[] objArray1 = new object[] { eventArgs };
                base.BeginInvoke(this.stateChangedDelegate, objArray1);
            }
        }

        public void PostNotification(QueueElement[] elements)
        {
            if (this.syncNoteReceiver != null)
            {
                object[] objArray1 = new object[] { elements };
                base.BeginInvoke(this.notificationQueueDelegate, objArray1);
            }
        }

        public void PostNotification(int handle, long timeStamp, int length, NotificationEntry entry, bool bError)
        {
            if (this.syncNoteReceiver != null)
            {
                object[] objArray1 = new object[] { handle, timeStamp, length, entry, bError };
                base.BeginInvoke(this.notificationDelegate, objArray1);
            }
        }

        public void PostRouterNotification(AmsRouterState state)
        {
            if (this.syncMsgReceiver != null)
            {
                object[] objArray1 = new object[] { state };
                base.BeginInvoke(this.routerNotificationDelegate, objArray1);
            }
        }

        public void PostSymbolVersionChanged(AdsSymbolVersionChangedEventArgs eventArgs)
        {
            if (this.syncMsgReceiver != null)
            {
                object[] objArray1 = new object[] { eventArgs };
                base.BeginInvoke(this.symbolVersionChangedDelegate, objArray1);
            }
        }

        private delegate void NotificationDelegate(int handle, long timeStamp, int length, NotificationEntry entry, bool bError);

        private delegate void NotificationQueueDelegate(QueueElement[] elements);

        private delegate void RouterNotificationDelegate(AmsRouterState state);

        private delegate void StateChangedDelegate(AdsStateChangedEventArgs eventArgs);

        private delegate void SymbolVersionChangedDelegate(AdsSymbolVersionChangedEventArgs eventArgs);
    }
}

