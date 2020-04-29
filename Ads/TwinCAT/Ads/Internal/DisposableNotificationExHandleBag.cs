namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using TwinCAT.Ads;
    using TwinCAT.Ads.SumCommand;
    using TwinCAT.TypeSystem;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public sealed class DisposableNotificationExHandleBag : DisposableHandleBag
    {
        public DisposableNotificationExHandleBag(IAdsConnection client, IDictionary<string, AnyTypeSpecifier> dict, NotificationSettings settings, object userData) : base(client)
        {
            if (dict == null)
            {
                throw new ArgumentNullException("dict");
            }
            if (dict.Count == 0)
            {
                throw new ArgumentOutOfRangeException("dict");
            }
            base.handleDict = new SumHandleList();
            base.validHandleDict = new Dictionary<string, uint>();
            foreach (KeyValuePair<string, AnyTypeSpecifier> pair in dict)
            {
                uint handle = 0;
                AnyTypeSpecifier specifier = pair.Value;
                Type tp = null;
                int[] args = null;
                specifier.GetAnyTypeArgs(out tp, out args);
                AdsErrorCode errorCode = client.TryAddDeviceNotificationEx(pair.Key, settings, userData, tp, args, out handle);
                base.handleDict.Add(new SumHandleInstancePathEntry(pair.Key, handle, errorCode));
                if (errorCode == AdsErrorCode.NoError)
                {
                    base.validHandleDict.Add(pair.Key, handle);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (KeyValuePair<string, uint> pair in base.validHandleDict)
                {
                    AdsErrorCode code = base.connection.TryDeleteDeviceNotification(pair.Value);
                }
            }
        }
    }
}

