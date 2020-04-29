namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using TwinCAT.Ads;

    internal class TcAdsSyncPortRouter : TcAdsSyncPort
    {
        public TcAdsSyncPortRouter(AmsAddress addr, TcLocalSystem localSystem, INotificationReceiver iNoteReceiver, bool clientCycle, bool synchronize) : base(addr, localSystem, iNoteReceiver, clientCycle, synchronize)
        {
            Module.Trace.TraceVerbose($"ID: ({base.Id:d})");
        }

        [SecuritySafeCritical]
        public override unsafe AdsErrorCode AddDeviceNotification(uint indexGroup, uint indexOffset, AdsNotificationAttrib* noteAttrib, TcAdsDllWrapper.AdsNotificationDelegate noteFunc, int hUser, bool throwAdsException, out int hNotification)
        {
            byte[] buffer;
            int pNotification = 0;
            IntPtr functionPointerForDelegate = Marshal.GetFunctionPointerForDelegate(noteFunc);
            AdsErrorCode noError = AdsErrorCode.NoError;
            if (((buffer = AmsAddressMarshaller.Marshal(base.address)) == null) || (buffer.Length == 0))
            {
                numRef = null;
            }
            else
            {
                numRef = buffer;
            }
            noError = TcAdsDllWrapper.UnsafeNativeMethods.AdsSyncAddDeviceNotificationReqEx(base._port, numRef, indexGroup, indexOffset, noteAttrib, functionPointerForDelegate, hUser, &pNotification);
            fixed (byte* numRef = null)
            {
                base.OnHandleCommunicationResult(noError, throwAdsException);
                hNotification = pNotification;
                return noError;
            }
        }

        [SecuritySafeCritical]
        public override unsafe AdsErrorCode AmsPortEnabled(bool throwAdsException, out bool enabled)
        {
            int pbEnabled = 0;
            AdsErrorCode adsErrorCode = TcAdsDllWrapper.UnsafeNativeMethods.AdsAmsPortEnabledEx(base._port, &pbEnabled);
            if ((adsErrorCode != AdsErrorCode.NoError) & throwAdsException)
            {
                ThrowAdsException(adsErrorCode);
            }
            enabled = pbEnabled != 0;
            return adsErrorCode;
        }

        [SecuritySafeCritical]
        public override unsafe AdsErrorCode DelDeviceNotification(int hNotification, bool throwAdsException)
        {
            byte[] buffer;
            AdsErrorCode noError = AdsErrorCode.NoError;
            if (((buffer = AmsAddressMarshaller.Marshal(base.address)) == null) || (buffer.Length == 0))
            {
                numRef = null;
            }
            else
            {
                numRef = buffer;
            }
            noError = TcAdsDllWrapper.UnsafeNativeMethods.AdsSyncDelDeviceNotificationReqEx(base._port, numRef, hNotification);
            fixed (byte* numRef = null)
            {
                base.OnHandleCommunicationResult(noError, throwAdsException);
                return noError;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        [SecuritySafeCritical]
        protected override unsafe AdsErrorCode GetLocalAddress(byte* data, bool throwAdsException)
        {
            AdsErrorCode adsErrorCode = TcAdsDllWrapper.UnsafeNativeMethods.AdsGetLocalAddressEx(base._port, (void*) data);
            if ((adsErrorCode != AdsErrorCode.NoError) & throwAdsException)
            {
                ThrowAdsException(adsErrorCode);
            }
            return adsErrorCode;
        }

        [SecuritySafeCritical]
        protected override unsafe AdsErrorCode GetTimeout(bool throwAdsException, out int timeout)
        {
            int num;
            AdsErrorCode adsErrorCode = TcAdsDllWrapper.UnsafeNativeMethods.AdsSyncGetTimeoutEx(base._port, &num);
            if ((adsErrorCode != AdsErrorCode.NoError) & throwAdsException)
            {
                ThrowAdsException(adsErrorCode);
            }
            timeout = num;
            return adsErrorCode;
        }

        protected override AdsErrorCode OnClosePort() => 
            this.AdsPortCloseEx(base._port, false);

        protected override AdsErrorCode OnOpenPort()
        {
            base._port = this.AdsPortOpenEx();
            return ((base._port != 0) ? AdsErrorCode.NoError : AdsErrorCode.ClientPortNotOpen);
        }

        [SecuritySafeCritical]
        public override unsafe AdsErrorCode Read(uint indexGroup, uint indexOffset, int length, void* data, bool throwAdsException, out int dataRead)
        {
            byte[] buffer;
            int pcbReturn = 0;
            AdsErrorCode noError = AdsErrorCode.NoError;
            if (((buffer = AmsAddressMarshaller.Marshal(base.address)) == null) || (buffer.Length == 0))
            {
                numRef = null;
            }
            else
            {
                numRef = buffer;
            }
            noError = TcAdsDllWrapper.UnsafeNativeMethods.AdsSyncReadReqEx2(base._port, numRef, indexGroup, indexOffset, length, data, &pcbReturn);
            fixed (byte* numRef = null)
            {
                base.OnHandleCommunicationResult(noError, throwAdsException);
                dataRead = pcbReturn;
                return noError;
            }
        }

        [SecuritySafeCritical]
        protected override unsafe AdsErrorCode ReadDeviceInfo(byte* devName, byte* version, bool throwAdsException)
        {
            byte[] buffer;
            AdsErrorCode noError = AdsErrorCode.NoError;
            if (((buffer = AmsAddressMarshaller.Marshal(base.address)) == null) || (buffer.Length == 0))
            {
                numRef = null;
            }
            else
            {
                numRef = buffer;
            }
            noError = TcAdsDllWrapper.UnsafeNativeMethods.AdsSyncReadDeviceInfoReqEx(base._port, numRef, devName, version);
            fixed (byte* numRef = null)
            {
                base.OnHandleCommunicationResult(noError, throwAdsException);
                return noError;
            }
        }

        [SecuritySafeCritical]
        public override unsafe AdsErrorCode ReadState(bool throwAdsException, out StateInfo stateInfo)
        {
            StateInfo info;
            byte[] buffer;
            AdsErrorCode noError = AdsErrorCode.NoError;
            if (((buffer = AmsAddressMarshaller.Marshal(base.address)) == null) || (buffer.Length == 0))
            {
                numRef = null;
            }
            else
            {
                numRef = buffer;
            }
            noError = TcAdsDllWrapper.UnsafeNativeMethods.AdsSyncReadStateReqEx(base._port, numRef, &info.adsState, &info.deviceState);
            fixed (byte* numRef = null)
            {
                base.OnHandleCommunicationResult(noError, throwAdsException);
                stateInfo = info;
                return noError;
            }
        }

        [SecuritySafeCritical]
        public override unsafe AdsErrorCode ReadWrite(uint indexGroup, uint indexOffset, int readLength, void* readData, int writeLength, void* writeData, bool throwAdsException, out int dataRead)
        {
            byte[] buffer;
            int pcbReturn = 0;
            AdsErrorCode noError = AdsErrorCode.NoError;
            if (((buffer = AmsAddressMarshaller.Marshal(base.address)) == null) || (buffer.Length == 0))
            {
                numRef = null;
            }
            else
            {
                numRef = buffer;
            }
            noError = TcAdsDllWrapper.UnsafeNativeMethods.AdsSyncReadWriteReqEx2(base._port, numRef, indexGroup, indexOffset, readLength, readData, writeLength, writeData, &pcbReturn);
            fixed (byte* numRef = null)
            {
                base.OnHandleCommunicationResult(noError, throwAdsException);
                dataRead = pcbReturn;
                return noError;
            }
        }

        [SecuritySafeCritical]
        protected override AdsErrorCode SetTimeout(int timeout, bool throwAdsException)
        {
            AdsErrorCode adsErrorCode = TcAdsDllWrapper.UnsafeNativeMethods.AdsSyncSetTimeoutEx(base._port, timeout);
            if ((adsErrorCode != AdsErrorCode.NoError) & throwAdsException)
            {
                ThrowAdsException(adsErrorCode);
            }
            return adsErrorCode;
        }

        [SecuritySafeCritical]
        public override unsafe AdsErrorCode Write(uint indexGroup, uint indexOffset, int length, void* data, bool throwAdsException)
        {
            byte[] buffer;
            AdsErrorCode noError = AdsErrorCode.NoError;
            if (((buffer = AmsAddressMarshaller.Marshal(base.address)) == null) || (buffer.Length == 0))
            {
                numRef = null;
            }
            else
            {
                numRef = buffer;
            }
            noError = TcAdsDllWrapper.UnsafeNativeMethods.AdsSyncWriteReqEx(base._port, numRef, indexGroup, indexOffset, length, data);
            fixed (byte* numRef = null)
            {
                base.OnHandleCommunicationResult(noError, throwAdsException);
                return noError;
            }
        }

        protected override unsafe AdsErrorCode WriteControl(AdsState adsState, short deviceState, int length, void* data, bool throwAdsException)
        {
            byte[] buffer;
            AdsErrorCode noError = AdsErrorCode.NoError;
            if (((buffer = AmsAddressMarshaller.Marshal(base.address)) == null) || (buffer.Length == 0))
            {
                numRef = null;
            }
            else
            {
                numRef = buffer;
            }
            noError = TcAdsDllWrapper.UnsafeNativeMethods.AdsSyncWriteControlReqEx(base._port, numRef, adsState, deviceState, length, data);
            fixed (byte* numRef = null)
            {
                base.OnHandleCommunicationResult(noError, throwAdsException);
                return noError;
            }
        }
    }
}

