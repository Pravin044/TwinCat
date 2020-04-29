namespace TwinCAT.Ads
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface IAdsNotifications
    {
        event AdsNotificationEventHandler AdsNotification;

        event AdsNotificationErrorEventHandler AdsNotificationError;

        event AdsNotificationExEventHandler AdsNotificationEx;

        int AddDeviceNotification(string variableName, AdsStream dataStream, AdsTransMode transMode, int cycleTime, int maxDelay, object userData);
        int AddDeviceNotification(uint indexGroup, uint indexOffset, AdsStream dataStream, AdsTransMode transMode, int cycleTime, int maxDelay, object userData);
        int AddDeviceNotification(string variableName, AdsStream dataStream, int offset, int length, AdsTransMode transMode, int cycleTime, int maxDelay, object userData);
        int AddDeviceNotification(uint indexGroup, uint indexOffset, AdsStream dataStream, int offset, int length, AdsTransMode transMode, int cycleTime, int maxDelay, object userData);
        int AddDeviceNotificationEx(string variableName, AdsTransMode transMode, int cycleTime, int maxDelay, object userData, Type type);
        int AddDeviceNotificationEx(string variableName, AdsTransMode transMode, int cycleTime, int maxDelay, object userData, Type type, int[] args);
        int AddDeviceNotificationEx(uint indexGroup, uint indexOffset, AdsTransMode transMode, int cycleTime, int maxDelay, object userData, Type type);
        int AddDeviceNotificationEx(uint indexGroup, uint indexOffset, AdsTransMode transMode, int cycleTime, int maxDelay, object userData, Type type, int[] args);
        void DeleteDeviceNotification(int notificationHandle);
        AdsErrorCode TryAddDeviceNotification(string variableName, AdsStream dataStream, int offset, int length, NotificationSettings settings, object userData, out uint handle);
        AdsErrorCode TryAddDeviceNotificationEx(string variableName, NotificationSettings settings, object userData, Type type, int[] args, out uint handle);
        AdsErrorCode TryDeleteDeviceNotification(uint notificationHandle);
    }
}

