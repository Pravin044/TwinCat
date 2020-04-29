namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using TwinCAT.Ads;
    using TwinCAT.TypeSystem;

    internal abstract class TcAdsDllWrapper : IDisposable
    {
        protected bool _disposed;
        protected AmsAddress address;
        private static int s_idCount = 1;
        private int _id;
        private System.Text.Encoding _encoding;

        protected TcAdsDllWrapper()
        {
            this._id = -1;
            this._encoding = System.Text.Encoding.Default;
            s_idCount++;
            this._id = s_idCount;
            Module.Trace.TraceVerbose($"ID: {this._id:d}");
        }

        public TcAdsDllWrapper(AmsAddress addr) : this()
        {
            Module.Trace.TraceVerbose($"ID: {this._id:d}, Address: {addr.ToString()}");
            this.address = addr.Clone();
        }

        [SecuritySafeCritical]
        public virtual unsafe AdsErrorCode AddDeviceNotification(uint indexGroup, uint indexOffset, AdsNotificationAttrib* noteAttrib, AdsNotificationDelegate noteFunc, int hUser, bool throwAdsException, out int hNotification)
        {
            int num;
            byte[] buffer;
            IntPtr functionPointerForDelegate = Marshal.GetFunctionPointerForDelegate(noteFunc);
            AdsErrorCode noError = AdsErrorCode.NoError;
            if (((buffer = AmsAddressMarshaller.Marshal(this.address)) == null) || (buffer.Length == 0))
            {
                numRef = null;
            }
            else
            {
                numRef = buffer;
            }
            noError = UnsafeNativeMethods.AdsSyncAddDeviceNotificationReq(numRef, indexGroup, indexOffset, noteAttrib, functionPointerForDelegate, hUser, &num);
            fixed (byte* numRef = null)
            {
                this.OnHandleCommunicationResult(noError, throwAdsException);
                hNotification = num;
                return noError;
            }
        }

        [SecuritySafeCritical]
        protected virtual AdsErrorCode AdsPortClose(bool throwAdsException)
        {
            AdsErrorCode internalError = AdsErrorCode.InternalError;
            try
            {
                internalError = UnsafeNativeMethods.AdsPortClose();
            }
            catch (Exception exception)
            {
                object[] args = new object[] { exception.Message };
                Module.Trace.TraceError("Closing AdsPort failed with error '{0}.", args);
            }
            if (internalError == AdsErrorCode.NoError)
            {
                Module.Trace.TraceInformation("AdsPort closed.");
            }
            else
            {
                object[] args = new object[] { internalError.ToString() };
                Module.Trace.TraceError("Closing AdsPort failed with error '{0}.", args);
            }
            if ((internalError != AdsErrorCode.NoError) & throwAdsException)
            {
                ThrowAdsException(internalError);
            }
            return internalError;
        }

        [SecuritySafeCritical]
        protected virtual AdsErrorCode AdsPortCloseEx(int port, bool throwAdsException)
        {
            AdsErrorCode adsErrorCode = UnsafeNativeMethods.AdsPortCloseEx(port);
            if ((adsErrorCode != AdsErrorCode.NoError) & throwAdsException)
            {
                ThrowAdsException(adsErrorCode);
            }
            return adsErrorCode;
        }

        [SecuritySafeCritical]
        protected virtual int AdsPortOpen()
        {
            int num;
            try
            {
                num = UnsafeNativeMethods.AdsPortOpen();
                object[] args = new object[] { num };
                Module.Trace.TraceInformation("Ads Port '{0}' opened!", args);
            }
            catch (Exception exception)
            {
                object[] args = new object[] { exception.Message };
                Module.Trace.TraceWarning("AdsPortOpen failed: {0}", args);
                try
                {
                    UnsafeNativeMethods.AdsGetDllVersion();
                }
                catch (Exception exception1)
                {
                    AdsInitializeException ex = new AdsInitializeException(exception1);
                    Module.Trace.TraceError(ex);
                    throw ex;
                }
                throw;
            }
            return num;
        }

        [SecuritySafeCritical]
        protected virtual unsafe int AdsPortOpenEx()
        {
            int num;
            try
            {
                num = UnsafeNativeMethods.AdsPortOpenEx();
                if (num <= 0)
                {
                    Module.Trace.TraceWarning("AdsPortOpenEx couldn't open a port!");
                }
                else
                {
                    object[] args = new object[] { num };
                    Module.Trace.TraceInformation("Ads Port '{0}' opened!", args);
                }
            }
            catch (Exception exception)
            {
                object[] args = new object[] { exception.Message };
                Module.Trace.TraceWarning("AdsPortOpenEx failed: {0}", args);
                try
                {
                    UnsafeNativeMethods.AdsGetDllVersion();
                }
                catch (Exception exception1)
                {
                    AdsInitializeException ex = new AdsInitializeException(exception1);
                    Module.Trace.TraceError(ex);
                    throw ex;
                }
                try
                {
                    int num2;
                    UnsafeNativeMethods.AdsSyncGetTimeout(&num2);
                }
                catch (Exception exception4)
                {
                    AdsInitializeException ex = null;
                    ex = new AdsInitializeException("Wrong version of 'TcAdsDll.dll'! Install version 2.0.0.1 or higher!", exception4);
                    Module.Trace.TraceError(ex);
                    throw ex;
                }
                throw;
            }
            return num;
        }

        [SecuritySafeCritical]
        public virtual unsafe AdsErrorCode AmsPortEnabled(bool throwAdsException, out bool enabled)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(base.GetType().ToString());
            }
            int pbEnabled = 0;
            AdsErrorCode adsErrorCode = UnsafeNativeMethods.AdsAmsPortEnabled(&pbEnabled);
            if ((adsErrorCode != AdsErrorCode.NoError) & throwAdsException)
            {
                ThrowAdsException(adsErrorCode);
            }
            enabled = pbEnabled != 0;
            return adsErrorCode;
        }

        [SecuritySafeCritical]
        public virtual AdsErrorCode AmsRegisterRouterNotification(AmsRouterNotificationDelegate pNoteFunc, bool throwAdsException)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(base.GetType().ToString());
            }
            AdsErrorCode result = UnsafeNativeMethods.AdsAmsRegisterRouterNotification(pNoteFunc);
            this.OnHandleCommunicationResult(result, throwAdsException);
            return result;
        }

        [SecuritySafeCritical]
        public virtual AdsErrorCode AmsUnRegisterRouterNotification(bool throwAdsException)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(base.GetType().ToString());
            }
            AdsErrorCode internalError = AdsErrorCode.InternalError;
            try
            {
                internalError = UnsafeNativeMethods.AdsAmsUnRegisterRouterNotification();
            }
            catch (Exception)
            {
            }
            this.OnHandleCommunicationResult(internalError, throwAdsException);
            return internalError;
        }

        private IntPtr CreateArrayElementIntPtr(IntPtr ptr, int element, int elementSize) => 
            ((IntPtr.Size != 8) ? new IntPtr(ptr.ToInt32() + (element * elementSize)) : new IntPtr(ptr.ToInt64() + (element * elementSize)));

        [SecuritySafeCritical]
        public virtual unsafe AdsErrorCode DelDeviceNotification(int hNotification, bool throwAdsException)
        {
            byte[] buffer;
            AdsErrorCode noError = AdsErrorCode.NoError;
            if (((buffer = AmsAddressMarshaller.Marshal(this.address)) == null) || (buffer.Length == 0))
            {
                numRef = null;
            }
            else
            {
                numRef = buffer;
            }
            noError = UnsafeNativeMethods.AdsSyncDelDeviceNotificationReq(numRef, hNotification);
            fixed (byte* numRef = null)
            {
                this.OnHandleCommunicationResult(noError, throwAdsException);
                return noError;
            }
        }

        public void Dispose()
        {
            TcAdsDllWrapper wrapper = this;
            lock (wrapper)
            {
                Module.Trace.TraceVerbose($"ID: ({this._id:d})");
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                bool flag1 = disposing;
            }
            this._disposed = true;
        }

        ~TcAdsDllWrapper()
        {
            Module.Trace.TraceVerbose($"TcAdsDllWrapper.finalizer({this._id:d})");
            this.Dispose(false);
        }

        [SecuritySafeCritical]
        protected virtual unsafe AdsErrorCode GetLocalAddress(byte* data, bool throwAdsException)
        {
            AdsErrorCode adsErrorCode = UnsafeNativeMethods.AdsGetLocalAddress(data);
            if ((adsErrorCode != AdsErrorCode.NoError) & throwAdsException)
            {
                ThrowAdsException(adsErrorCode);
            }
            return adsErrorCode;
        }

        [SecurityCritical]
        protected unsafe byte[] GetLocalNetId()
        {
            byte[] buffer2;
            byte[] buffer = new byte[6];
            if (((buffer2 = buffer) == null) || (buffer2.Length == 0))
            {
                numRef = null;
            }
            else
            {
                numRef = buffer2;
            }
            this.GetLocalAddress(numRef, true);
            fixed (byte* numRef = null)
            {
                return buffer;
            }
        }

        [SecuritySafeCritical]
        protected virtual unsafe AdsErrorCode GetTimeout(bool throwAdsException, out int timeout)
        {
            int num;
            if (this._disposed)
            {
                throw new ObjectDisposedException(base.GetType().ToString());
            }
            AdsErrorCode adsErrorCode = UnsafeNativeMethods.AdsSyncGetTimeout(&num);
            if ((adsErrorCode != AdsErrorCode.NoError) & throwAdsException)
            {
                ThrowAdsException(adsErrorCode);
            }
            timeout = num;
            return adsErrorCode;
        }

        protected bool IsLocalNetId(AmsNetId netId, AmsNetId localNetId)
        {
            bool flag = true;
            if (netId != null)
            {
                int index = 0;
                while (true)
                {
                    if (index < 6)
                    {
                        if (netId.netId[index] == 0)
                        {
                            index++;
                            continue;
                        }
                        flag = false;
                    }
                    if (!flag)
                    {
                        if (((netId.netId[0] == 0x7f) && ((netId.netId[1] == 0) && ((netId.netId[2] == 0) && ((netId.netId[3] == 1) && (netId.netId[4] == 1))))) && (netId.netId[5] == 1))
                        {
                            return true;
                        }
                        if (((netId.netId[0] == localNetId.netId[0]) && ((netId.netId[1] == localNetId.netId[1]) && ((netId.netId[2] == localNetId.netId[2]) && ((netId.netId[3] == localNetId.netId[3]) && (netId.netId[4] == localNetId.netId[4]))))) && (netId.netId[5] == localNetId.netId[5]))
                        {
                            return true;
                        }
                    }
                    break;
                }
            }
            return flag;
        }

        protected void OnHandleCommunicationResult(AdsErrorCode result, bool throwAdsException)
        {
            if ((result != AdsErrorCode.NoError) && throwAdsException)
            {
                ThrowAdsException(result);
            }
        }

        [SecuritySafeCritical]
        public virtual unsafe AdsErrorCode Read(uint indexGroup, uint indexOffset, int length, void* data, bool throwAdsException, out int dataRead)
        {
            int num;
            byte[] buffer;
            AdsErrorCode noError = AdsErrorCode.NoError;
            if (((buffer = AmsAddressMarshaller.Marshal(this.address)) == null) || (buffer.Length == 0))
            {
                numRef = null;
            }
            else
            {
                numRef = buffer;
            }
            noError = UnsafeNativeMethods.AdsSyncReadReqEx(numRef, indexGroup, indexOffset, length, data, &num);
            fixed (byte* numRef = null)
            {
                this.OnHandleCommunicationResult(noError, throwAdsException);
                dataRead = num;
                return noError;
            }
        }

        [SecuritySafeCritical]
        public unsafe AdsErrorCode Read(uint indexGroup, uint indexOffset, int offset, int length, byte[] data, bool throwAdsException, out int dataRead)
        {
            ref byte pinned numRef;
            byte[] buffer;
            if (length == 0)
            {
                byte num;
                return this.Read(indexGroup, indexOffset, length, (void*) &num, throwAdsException, out dataRead);
            }
            if (((buffer = data) == null) || (buffer.Length == 0))
            {
                numRef = null;
            }
            else
            {
                numRef = buffer;
            }
            return this.Read(indexGroup, indexOffset, length, (void*) (numRef + offset), throwAdsException, out dataRead);
        }

        [SecuritySafeCritical]
        protected unsafe object ReadArrayOfBoolean(uint indexGroup, uint indexOffset, int[] elements, bool throwAdsException, out AdsErrorCode result)
        {
            object obj2 = null;
            int num;
            switch (elements.Length)
            {
                case 1:
                {
                    bool[] flagArray4;
                    bool[] flagArray = new bool[elements[0]];
                    if (((flagArray4 = flagArray) == null) || (flagArray4.Length == 0))
                    {
                        flagRef = null;
                    }
                    else
                    {
                        flagRef = flagArray4;
                    }
                    result = this.Read(indexGroup, indexOffset, flagArray.Length, (void*) flagRef, throwAdsException, out num);
                    fixed (bool* flagRef = null)
                    {
                        if (result == AdsErrorCode.NoError)
                        {
                            obj2 = flagArray;
                        }
                        break;
                    }
                }
                case 2:
                {
                    bool[,] flagArray5;
                    bool[,] flagArray2 = new bool[elements[0], elements[1]];
                    if (((flagArray5 = flagArray2) == null) || (flagArray5.Length == 0))
                    {
                        flagRef2 = null;
                    }
                    else
                    {
                        flagRef2 = (bool*) flagArray5[0, 0];
                    }
                    result = this.Read(indexGroup, indexOffset, flagArray2.Length, (void*) flagRef2, throwAdsException, out num);
                    fixed (bool* flagRef2 = null)
                    {
                        if (result == AdsErrorCode.NoError)
                        {
                            obj2 = flagArray2;
                        }
                        break;
                    }
                }
                case 3:
                {
                    bool[,,] flagArray6;
                    bool[,,] flagArray3 = new bool[elements[0], elements[1], elements[2]];
                    if (((flagArray6 = flagArray3) == null) || (flagArray6.Length == 0))
                    {
                        flagRef3 = null;
                    }
                    else
                    {
                        flagRef3 = (bool*) flagArray6[0, 0, 0];
                    }
                    result = this.Read(indexGroup, indexOffset, flagArray3.Length, (void*) flagRef3, throwAdsException, out num);
                    fixed (bool* flagRef3 = null)
                    {
                        if (result == AdsErrorCode.NoError)
                        {
                            obj2 = flagArray3;
                        }
                        break;
                    }
                }
                default:
                    throw new ArgumentException("Unable to marshal type.");
            }
            return obj2;
        }

        [SecuritySafeCritical]
        protected unsafe object ReadArrayOfInt16(uint indexGroup, uint indexOffset, int[] elements, bool throwAdsException, out AdsErrorCode result)
        {
            object obj2 = null;
            int num;
            switch (elements.Length)
            {
                case 1:
                {
                    short[] numArray4;
                    short[] numArray = new short[elements[0]];
                    if (((numArray4 = numArray) == null) || (numArray4.Length == 0))
                    {
                        numRef = null;
                    }
                    else
                    {
                        numRef = numArray4;
                    }
                    result = this.Read(indexGroup, indexOffset, numArray.Length * 2, (void*) numRef, throwAdsException, out num);
                    fixed (short* numRef = null)
                    {
                        if (result == AdsErrorCode.NoError)
                        {
                            obj2 = numArray;
                        }
                        break;
                    }
                }
                case 2:
                {
                    short[,] numArray5;
                    short[,] numArray2 = new short[elements[0], elements[1]];
                    if (((numArray5 = numArray2) == null) || (numArray5.Length == 0))
                    {
                        numRef2 = null;
                    }
                    else
                    {
                        numRef2 = (short*) numArray5[0, 0];
                    }
                    result = this.Read(indexGroup, indexOffset, numArray2.Length * 2, (void*) numRef2, throwAdsException, out num);
                    fixed (short* numRef2 = null)
                    {
                        if (result == AdsErrorCode.NoError)
                        {
                            obj2 = numArray2;
                        }
                        break;
                    }
                }
                case 3:
                {
                    short[,,] numArray6;
                    short[,,] numArray3 = new short[elements[0], elements[1], elements[2]];
                    if (((numArray6 = numArray3) == null) || (numArray6.Length == 0))
                    {
                        numRef3 = null;
                    }
                    else
                    {
                        numRef3 = (short*) numArray6[0, 0, 0];
                    }
                    result = this.Read(indexGroup, indexOffset, numArray3.Length * 2, (void*) numRef3, throwAdsException, out num);
                    fixed (short* numRef3 = null)
                    {
                        if (result == AdsErrorCode.NoError)
                        {
                            obj2 = numArray3;
                        }
                        break;
                    }
                }
                default:
                    throw new ArgumentException("Unable to marshal type.");
            }
            return obj2;
        }

        [SecuritySafeCritical]
        protected unsafe object ReadArrayOfInt32(uint indexGroup, uint indexOffset, int[] elements, bool throwAdsException, out AdsErrorCode result)
        {
            object obj2 = null;
            int num;
            switch (elements.Length)
            {
                case 1:
                {
                    int[] numArray4;
                    int[] numArray = new int[elements[0]];
                    if (((numArray4 = numArray) == null) || (numArray4.Length == 0))
                    {
                        numRef = null;
                    }
                    else
                    {
                        numRef = numArray4;
                    }
                    result = this.Read(indexGroup, indexOffset, numArray.Length * 4, (void*) numRef, throwAdsException, out num);
                    fixed (int* numRef = null)
                    {
                        if (result == AdsErrorCode.NoError)
                        {
                            obj2 = numArray;
                        }
                        break;
                    }
                }
                case 2:
                {
                    int[,] numArray5;
                    int[,] numArray2 = new int[elements[0], elements[1]];
                    if (((numArray5 = numArray2) == null) || (numArray5.Length == 0))
                    {
                        numRef2 = null;
                    }
                    else
                    {
                        numRef2 = (int*) numArray5[0, 0];
                    }
                    result = this.Read(indexGroup, indexOffset, numArray2.Length * 4, (void*) numRef2, throwAdsException, out num);
                    fixed (int* numRef2 = null)
                    {
                        if (result == AdsErrorCode.NoError)
                        {
                            obj2 = numArray2;
                        }
                        break;
                    }
                }
                case 3:
                {
                    int[,,] numArray6;
                    int[,,] numArray3 = new int[elements[0], elements[1], elements[2]];
                    if (((numArray6 = numArray3) == null) || (numArray6.Length == 0))
                    {
                        numRef3 = null;
                    }
                    else
                    {
                        numRef3 = (int*) numArray6[0, 0, 0];
                    }
                    result = this.Read(indexGroup, indexOffset, numArray3.Length * 4, (void*) numRef3, throwAdsException, out num);
                    fixed (int* numRef3 = null)
                    {
                        if (result == AdsErrorCode.NoError)
                        {
                            obj2 = numArray3;
                        }
                        break;
                    }
                }
                default:
                    throw new ArgumentException("Unable to marshal type.");
            }
            return obj2;
        }

        [SecuritySafeCritical]
        protected unsafe object ReadArrayOfInt64(uint indexGroup, uint indexOffset, int[] elements, bool throwAdsException, out AdsErrorCode result)
        {
            object obj2 = null;
            int num;
            switch (elements.Length)
            {
                case 1:
                {
                    long[] numArray4;
                    long[] numArray = new long[elements[0]];
                    if (((numArray4 = numArray) == null) || (numArray4.Length == 0))
                    {
                        numRef = null;
                    }
                    else
                    {
                        numRef = numArray4;
                    }
                    result = this.Read(indexGroup, indexOffset, numArray.Length * 8, (void*) numRef, throwAdsException, out num);
                    fixed (long* numRef = null)
                    {
                        if (result == AdsErrorCode.NoError)
                        {
                            obj2 = numArray;
                        }
                        break;
                    }
                }
                case 2:
                {
                    long[,] numArray5;
                    long[,] numArray2 = new long[elements[0], elements[1]];
                    if (((numArray5 = numArray2) == null) || (numArray5.Length == 0))
                    {
                        numRef2 = null;
                    }
                    else
                    {
                        numRef2 = (long*) numArray5[0, 0];
                    }
                    result = this.Read(indexGroup, indexOffset, numArray2.Length * 8, (void*) numRef2, throwAdsException, out num);
                    fixed (long* numRef2 = null)
                    {
                        if (result == AdsErrorCode.NoError)
                        {
                            obj2 = numArray2;
                        }
                        break;
                    }
                }
                case 3:
                {
                    long[,,] numArray6;
                    long[,,] numArray3 = new long[elements[0], elements[1], elements[2]];
                    if (((numArray6 = numArray3) == null) || (numArray6.Length == 0))
                    {
                        numRef3 = null;
                    }
                    else
                    {
                        numRef3 = (long*) numArray6[0, 0, 0];
                    }
                    result = this.Read(indexGroup, indexOffset, numArray3.Length * 8, (void*) numRef3, throwAdsException, out num);
                    fixed (long* numRef3 = null)
                    {
                        if (result == AdsErrorCode.NoError)
                        {
                            obj2 = numArray3;
                        }
                        break;
                    }
                }
                default:
                    throw new ArgumentException("Unable to marshal type.");
            }
            return obj2;
        }

        [SecuritySafeCritical]
        protected unsafe object ReadArrayOfInt8(uint indexGroup, uint indexOffset, int[] elements, bool throwAdsException, out AdsErrorCode result)
        {
            object obj2 = null;
            int num;
            switch (elements.Length)
            {
                case 1:
                {
                    sbyte[] numArray4;
                    sbyte[] numArray = new sbyte[elements[0]];
                    if (((numArray4 = numArray) == null) || (numArray4.Length == 0))
                    {
                        numRef = null;
                    }
                    else
                    {
                        numRef = numArray4;
                    }
                    result = this.Read(indexGroup, indexOffset, numArray.Length, (void*) numRef, throwAdsException, out num);
                    fixed (sbyte* numRef = null)
                    {
                        if (result == AdsErrorCode.NoError)
                        {
                            obj2 = numArray;
                        }
                        break;
                    }
                }
                case 2:
                {
                    sbyte[,] numArray5;
                    sbyte[,] numArray2 = new sbyte[elements[0], elements[1]];
                    if (((numArray5 = numArray2) == null) || (numArray5.Length == 0))
                    {
                        numRef2 = null;
                    }
                    else
                    {
                        numRef2 = (sbyte*) numArray5[0, 0];
                    }
                    result = this.Read(indexGroup, indexOffset, numArray2.Length, (void*) numRef2, throwAdsException, out num);
                    fixed (sbyte* numRef2 = null)
                    {
                        if (result == AdsErrorCode.NoError)
                        {
                            obj2 = numArray2;
                        }
                        break;
                    }
                }
                case 3:
                {
                    sbyte[,,] numArray6;
                    sbyte[,,] numArray3 = new sbyte[elements[0], elements[1], elements[2]];
                    if (((numArray6 = numArray3) == null) || (numArray6.Length == 0))
                    {
                        numRef3 = null;
                    }
                    else
                    {
                        numRef3 = (sbyte*) numArray6[0, 0, 0];
                    }
                    result = this.Read(indexGroup, indexOffset, numArray3.Length, (void*) numRef3, throwAdsException, out num);
                    fixed (sbyte* numRef3 = null)
                    {
                        if (result == AdsErrorCode.NoError)
                        {
                            obj2 = numArray3;
                        }
                        break;
                    }
                }
                default:
                    throw new ArgumentException("Unable to marshal type.");
            }
            return obj2;
        }

        [SecuritySafeCritical]
        protected unsafe object ReadArrayOfReal32(uint indexGroup, uint indexOffset, int[] elements, bool throwAdsException, out AdsErrorCode result)
        {
            object obj2 = null;
            int num;
            switch (elements.Length)
            {
                case 1:
                {
                    float[] numArray4;
                    float[] numArray = new float[elements[0]];
                    if (((numArray4 = numArray) == null) || (numArray4.Length == 0))
                    {
                        numRef = null;
                    }
                    else
                    {
                        numRef = numArray4;
                    }
                    result = this.Read(indexGroup, indexOffset, numArray.Length * 4, (void*) numRef, throwAdsException, out num);
                    fixed (float* numRef = null)
                    {
                        if (result == AdsErrorCode.NoError)
                        {
                            obj2 = numArray;
                        }
                        break;
                    }
                }
                case 2:
                {
                    float[,] numArray5;
                    float[,] numArray2 = new float[elements[0], elements[1]];
                    if (((numArray5 = numArray2) == null) || (numArray5.Length == 0))
                    {
                        numRef2 = null;
                    }
                    else
                    {
                        numRef2 = (float*) numArray5[0, 0];
                    }
                    result = this.Read(indexGroup, indexOffset, numArray2.Length * 4, (void*) numRef2, throwAdsException, out num);
                    fixed (float* numRef2 = null)
                    {
                        if (result == AdsErrorCode.NoError)
                        {
                            obj2 = numArray2;
                        }
                        break;
                    }
                }
                case 3:
                {
                    float[,,] numArray6;
                    float[,,] numArray3 = new float[elements[0], elements[1], elements[2]];
                    if (((numArray6 = numArray3) == null) || (numArray6.Length == 0))
                    {
                        numRef3 = null;
                    }
                    else
                    {
                        numRef3 = (float*) numArray6[0, 0, 0];
                    }
                    result = this.Read(indexGroup, indexOffset, numArray3.Length * 4, (void*) numRef3, throwAdsException, out num);
                    fixed (float* numRef3 = null)
                    {
                        if (result == AdsErrorCode.NoError)
                        {
                            obj2 = numArray3;
                        }
                        break;
                    }
                }
                default:
                    throw new ArgumentException("Unable to marshal type.");
            }
            return obj2;
        }

        [SecuritySafeCritical]
        protected unsafe object ReadArrayOfReal64(uint indexGroup, uint indexOffset, int[] elements, bool throwAdsException, out AdsErrorCode result)
        {
            object obj2 = null;
            int num;
            switch (elements.Length)
            {
                case 1:
                {
                    double[] numArray4;
                    double[] numArray = new double[elements[0]];
                    if (((numArray4 = numArray) == null) || (numArray4.Length == 0))
                    {
                        numRef = null;
                    }
                    else
                    {
                        numRef = numArray4;
                    }
                    result = this.Read(indexGroup, indexOffset, numArray.Length * 8, (void*) numRef, throwAdsException, out num);
                    fixed (double* numRef = null)
                    {
                        if (result == AdsErrorCode.NoError)
                        {
                            obj2 = numArray;
                        }
                        break;
                    }
                }
                case 2:
                {
                    double[,] numArray5;
                    double[,] numArray2 = new double[elements[0], elements[1]];
                    if (((numArray5 = numArray2) == null) || (numArray5.Length == 0))
                    {
                        numRef2 = null;
                    }
                    else
                    {
                        numRef2 = (double*) numArray5[0, 0];
                    }
                    result = this.Read(indexGroup, indexOffset, numArray2.Length * 8, (void*) numRef2, throwAdsException, out num);
                    fixed (double* numRef2 = null)
                    {
                        if (result == AdsErrorCode.NoError)
                        {
                            obj2 = numArray2;
                        }
                        break;
                    }
                }
                case 3:
                {
                    double[,,] numArray6;
                    double[,,] numArray3 = new double[elements[0], elements[1], elements[2]];
                    if (((numArray6 = numArray3) == null) || (numArray6.Length == 0))
                    {
                        numRef3 = null;
                    }
                    else
                    {
                        numRef3 = (double*) numArray6[0, 0, 0];
                    }
                    result = this.Read(indexGroup, indexOffset, numArray3.Length * 8, (void*) numRef3, throwAdsException, out num);
                    fixed (double* numRef3 = null)
                    {
                        if (result == AdsErrorCode.NoError)
                        {
                            obj2 = numArray3;
                        }
                        break;
                    }
                }
                default:
                    throw new ArgumentException("Unable to marshal type.");
            }
            return obj2;
        }

        protected unsafe object ReadArrayOfString(uint indexGroup, uint indexOffset, int characters, int elements, bool throwAdsException, out AdsErrorCode result)
        {
            string[] strArray = new string[elements];
            int size = (characters + 1) * elements;
            IntPtr ptr = TcAdsDllMarshaller.Alloc(size);
            try
            {
                int num2;
                result = this.Read(indexGroup, indexOffset, size, ptr.ToPointer(), throwAdsException, out num2);
                if (result != AdsErrorCode.NoError)
                {
                    strArray = null;
                }
                else if (size != num2)
                {
                    result = AdsErrorCode.ClientInvalidParameter;
                    if (throwAdsException)
                    {
                        ThrowAdsException(result);
                    }
                }
                else
                {
                    for (int i = 0; i < elements; i++)
                    {
                        strArray[i] = TcAdsDllMarshaller.PtrToStringAnsi(this.CreateArrayElementIntPtr(ptr, i, characters + 1), characters + 1);
                        int index = strArray[i].IndexOf('\0');
                        if (index != -1)
                        {
                            strArray[i] = strArray[i].Substring(0, index);
                        }
                    }
                }
            }
            finally
            {
                TcAdsDllMarshaller.Free(ptr);
            }
            return strArray;
        }

        protected unsafe object ReadArrayOfStruct(uint indexGroup, uint indexOffset, Type elementType, int elements, bool throwAdsException, out AdsErrorCode result)
        {
            object obj2;
            Array array = Array.CreateInstance(elementType, elements);
            int elementSize = Marshal.SizeOf(elementType);
            int size = elementSize * elements;
            IntPtr ptr = TcAdsDllMarshaller.Alloc(size);
            try
            {
                int num;
                result = this.Read(indexGroup, indexOffset, size, ptr.ToPointer(), throwAdsException, out num);
                if (result == AdsErrorCode.NoError)
                {
                    if (size == num)
                    {
                        int index = 0;
                        while (true)
                        {
                            if (index >= elements)
                            {
                                obj2 = array;
                                break;
                            }
                            array.SetValue(Marshal.PtrToStructure(this.CreateArrayElementIntPtr(ptr, index, elementSize), elementType), index);
                            index++;
                        }
                        return obj2;
                    }
                    else
                    {
                        result = AdsErrorCode.ClientInvalidParameter;
                        if (throwAdsException)
                        {
                            ThrowAdsException(result);
                        }
                    }
                }
                obj2 = null;
            }
            finally
            {
                TcAdsDllMarshaller.Free(ptr);
            }
            return obj2;
        }

        [SecuritySafeCritical]
        protected unsafe object ReadArrayOfUInt16(uint indexGroup, uint indexOffset, int[] elements, bool throwAdsException, out AdsErrorCode result)
        {
            object obj2 = null;
            int num;
            switch (elements.Length)
            {
                case 1:
                {
                    ushort[] numArray4;
                    ushort[] numArray = new ushort[elements[0]];
                    if (((numArray4 = numArray) == null) || (numArray4.Length == 0))
                    {
                        numRef = null;
                    }
                    else
                    {
                        numRef = numArray4;
                    }
                    result = this.Read(indexGroup, indexOffset, numArray.Length * 2, (void*) numRef, throwAdsException, out num);
                    fixed (ushort* numRef = null)
                    {
                        if (result == AdsErrorCode.NoError)
                        {
                            obj2 = numArray;
                        }
                        break;
                    }
                }
                case 2:
                {
                    ushort[,] numArray5;
                    ushort[,] numArray2 = new ushort[elements[0], elements[1]];
                    if (((numArray5 = numArray2) == null) || (numArray5.Length == 0))
                    {
                        numRef2 = null;
                    }
                    else
                    {
                        numRef2 = (ushort*) numArray5[0, 0];
                    }
                    result = this.Read(indexGroup, indexOffset, numArray2.Length * 2, (void*) numRef2, throwAdsException, out num);
                    fixed (ushort* numRef2 = null)
                    {
                        if (result == AdsErrorCode.NoError)
                        {
                            obj2 = numArray2;
                        }
                        break;
                    }
                }
                case 3:
                {
                    ushort[,,] numArray6;
                    ushort[,,] numArray3 = new ushort[elements[0], elements[1], elements[2]];
                    if (((numArray6 = numArray3) == null) || (numArray6.Length == 0))
                    {
                        numRef3 = null;
                    }
                    else
                    {
                        numRef3 = (ushort*) numArray6[0, 0, 0];
                    }
                    result = this.Read(indexGroup, indexOffset, numArray3.Length * 2, (void*) numRef3, throwAdsException, out num);
                    fixed (ushort* numRef3 = null)
                    {
                        if (result == AdsErrorCode.NoError)
                        {
                            obj2 = numArray3;
                        }
                        break;
                    }
                }
                default:
                    throw new ArgumentException("Unable to marshal type.");
            }
            return obj2;
        }

        [SecuritySafeCritical]
        protected unsafe object ReadArrayOfUInt32(uint indexGroup, uint indexOffset, int[] elements, bool throwAdsException, out AdsErrorCode result)
        {
            object obj2 = null;
            int num;
            switch (elements.Length)
            {
                case 1:
                {
                    uint[] numArray4;
                    uint[] numArray = new uint[elements[0]];
                    if (((numArray4 = numArray) == null) || (numArray4.Length == 0))
                    {
                        numRef = null;
                    }
                    else
                    {
                        numRef = numArray4;
                    }
                    result = this.Read(indexGroup, indexOffset, numArray.Length * 4, (void*) numRef, throwAdsException, out num);
                    fixed (uint* numRef = null)
                    {
                        if (result == AdsErrorCode.NoError)
                        {
                            obj2 = numArray;
                        }
                        break;
                    }
                }
                case 2:
                {
                    uint[,] numArray5;
                    uint[,] numArray2 = new uint[elements[0], elements[1]];
                    if (((numArray5 = numArray2) == null) || (numArray5.Length == 0))
                    {
                        numRef2 = null;
                    }
                    else
                    {
                        numRef2 = (uint*) numArray5[0, 0];
                    }
                    result = this.Read(indexGroup, indexOffset, numArray2.Length * 4, (void*) numRef2, throwAdsException, out num);
                    fixed (uint* numRef2 = null)
                    {
                        if (result == AdsErrorCode.NoError)
                        {
                            obj2 = numArray2;
                        }
                        break;
                    }
                }
                case 3:
                {
                    uint[,,] numArray6;
                    uint[,,] numArray3 = new uint[elements[0], elements[1], elements[2]];
                    if (((numArray6 = numArray3) == null) || (numArray6.Length == 0))
                    {
                        numRef3 = null;
                    }
                    else
                    {
                        numRef3 = (uint*) numArray6[0, 0, 0];
                    }
                    result = this.Read(indexGroup, indexOffset, numArray3.Length * 4, (void*) numRef3, throwAdsException, out num);
                    fixed (uint* numRef3 = null)
                    {
                        if (result == AdsErrorCode.NoError)
                        {
                            obj2 = numArray3;
                        }
                        break;
                    }
                }
                default:
                    throw new ArgumentException("Unable to marshal type.");
            }
            return obj2;
        }

        [SecuritySafeCritical]
        protected unsafe object ReadArrayOfUInt64(uint indexGroup, uint indexOffset, int[] elements, bool throwAdsException, out AdsErrorCode result)
        {
            object obj2 = null;
            int num;
            switch (elements.Length)
            {
                case 1:
                {
                    ulong[] numArray4;
                    ulong[] numArray = new ulong[elements[0]];
                    if (((numArray4 = numArray) == null) || (numArray4.Length == 0))
                    {
                        numRef = null;
                    }
                    else
                    {
                        numRef = numArray4;
                    }
                    result = this.Read(indexGroup, indexOffset, numArray.Length * 8, (void*) numRef, throwAdsException, out num);
                    fixed (ulong* numRef = null)
                    {
                        if (result == AdsErrorCode.NoError)
                        {
                            obj2 = numArray;
                        }
                        break;
                    }
                }
                case 2:
                {
                    ulong[,] numArray5;
                    ulong[,] numArray2 = new ulong[elements[0], elements[1]];
                    if (((numArray5 = numArray2) == null) || (numArray5.Length == 0))
                    {
                        numRef2 = null;
                    }
                    else
                    {
                        numRef2 = (ulong*) numArray5[0, 0];
                    }
                    result = this.Read(indexGroup, indexOffset, numArray2.Length * 8, (void*) numRef2, throwAdsException, out num);
                    fixed (ulong* numRef2 = null)
                    {
                        if (result == AdsErrorCode.NoError)
                        {
                            obj2 = numArray2;
                        }
                        break;
                    }
                }
                case 3:
                {
                    ulong[,,] numArray6;
                    ulong[,,] numArray3 = new ulong[elements[0], elements[1], elements[2]];
                    if (((numArray6 = numArray3) == null) || (numArray6.Length == 0))
                    {
                        numRef3 = null;
                    }
                    else
                    {
                        numRef3 = (ulong*) numArray6[0, 0, 0];
                    }
                    result = this.Read(indexGroup, indexOffset, numArray3.Length * 8, (void*) numRef3, throwAdsException, out num);
                    fixed (ulong* numRef3 = null)
                    {
                        if (result == AdsErrorCode.NoError)
                        {
                            obj2 = numArray3;
                        }
                        break;
                    }
                }
                default:
                    throw new ArgumentException("Unable to marshal type.");
            }
            return obj2;
        }

        [SecuritySafeCritical]
        protected unsafe object ReadArrayOfUInt8(uint indexGroup, uint indexOffset, int[] elements, bool throwAdsException, out AdsErrorCode result)
        {
            object obj2 = null;
            int num;
            switch (elements.Length)
            {
                case 1:
                {
                    byte[] buffer4;
                    byte[] buffer = new byte[elements[0]];
                    if (((buffer4 = buffer) == null) || (buffer4.Length == 0))
                    {
                        numRef = null;
                    }
                    else
                    {
                        numRef = buffer4;
                    }
                    result = this.Read(indexGroup, indexOffset, buffer.Length, (void*) numRef, throwAdsException, out num);
                    fixed (byte* numRef = null)
                    {
                        if (result == AdsErrorCode.NoError)
                        {
                            obj2 = buffer;
                        }
                        break;
                    }
                }
                case 2:
                {
                    byte[,] buffer5;
                    byte[,] buffer2 = new byte[elements[0], elements[1]];
                    if (((buffer5 = buffer2) == null) || (buffer5.Length == 0))
                    {
                        numRef2 = null;
                    }
                    else
                    {
                        numRef2 = (byte*) buffer5[0, 0];
                    }
                    result = this.Read(indexGroup, indexOffset, buffer2.Length, (void*) numRef2, throwAdsException, out num);
                    fixed (byte* numRef2 = null)
                    {
                        if (result == AdsErrorCode.NoError)
                        {
                            obj2 = buffer2;
                        }
                        break;
                    }
                }
                case 3:
                {
                    byte[,,] buffer6;
                    byte[,,] buffer3 = new byte[elements[0], elements[1], elements[2]];
                    if (((buffer6 = buffer3) == null) || (buffer6.Length == 0))
                    {
                        numRef3 = null;
                    }
                    else
                    {
                        numRef3 = (byte*) buffer6[0, 0, 0];
                    }
                    result = this.Read(indexGroup, indexOffset, buffer3.Length, (void*) numRef3, throwAdsException, out num);
                    fixed (byte* numRef3 = null)
                    {
                        if (result == AdsErrorCode.NoError)
                        {
                            obj2 = buffer3;
                        }
                        break;
                    }
                }
                default:
                    throw new ArgumentException("Unable to marshal type.");
            }
            return obj2;
        }

        public unsafe bool ReadBoolean(uint indexGroup, uint indexOffset, bool throwAdsException, out AdsErrorCode result)
        {
            byte num;
            int num2;
            bool flag = false;
            result = this.Read(indexGroup, indexOffset, 1, (void*) &num, throwAdsException, out num2);
            if ((result == AdsErrorCode.NoError) && (num != 0))
            {
                flag = true;
            }
            return flag;
        }

        [SecuritySafeCritical]
        public unsafe AdsErrorCode ReadDeviceInfo(bool throwAdsException, out DeviceInfo deviceInfo)
        {
            byte[] buffer3;
            byte[] buffer4;
            deviceInfo = new DeviceInfo();
            byte[] bytes = new byte[0x10];
            byte[] buffer2 = new byte[6];
            if (((buffer3 = bytes) == null) || (buffer3.Length == 0))
            {
                numRef = null;
            }
            else
            {
                numRef = buffer3;
            }
            if (((buffer4 = buffer2) == null) || (buffer4.Length == 0))
            {
                numRef2 = null;
            }
            else
            {
                numRef2 = buffer4;
            }
            AdsErrorCode code = this.ReadDeviceInfo(numRef, numRef2, throwAdsException);
            if (code == AdsErrorCode.NoError)
            {
                deviceInfo.Version = TcAdsDllMarshaller.PtrToAdsVersion(numRef2);
                deviceInfo.Name = PlcStringConverter.UnmarshalAnsi(bytes);
            }
            fixed (byte* numRef = null)
            {
                fixed (byte* numRef2 = null)
                {
                    return code;
                }
            }
        }

        [SecuritySafeCritical]
        protected virtual unsafe AdsErrorCode ReadDeviceInfo(byte* devName, byte* version, bool throwAdsException)
        {
            byte[] buffer;
            AdsErrorCode noError = AdsErrorCode.NoError;
            if (((buffer = AmsAddressMarshaller.Marshal(this.address)) == null) || (buffer.Length == 0))
            {
                numRef = null;
            }
            else
            {
                numRef = buffer;
            }
            noError = UnsafeNativeMethods.AdsSyncReadDeviceInfoReq(numRef, devName, version);
            fixed (byte* numRef = null)
            {
                this.OnHandleCommunicationResult(noError, throwAdsException);
                return noError;
            }
        }

        public unsafe short ReadInt16(uint indexGroup, uint indexOffset, bool throwAdsException, out AdsErrorCode result)
        {
            short num;
            int num2;
            result = this.Read(indexGroup, indexOffset, 2, (void*) &num, throwAdsException, out num2);
            return num;
        }

        public unsafe int ReadInt32(uint indexGroup, uint indexOffset, bool throwAdsException, out AdsErrorCode result)
        {
            int num;
            int num2;
            result = this.Read(indexGroup, indexOffset, 4, (void*) &num, throwAdsException, out num2);
            return num;
        }

        public unsafe long ReadInt64(uint indexGroup, uint indexOffset, bool throwAdsException, out AdsErrorCode result)
        {
            long num;
            int num2;
            result = this.Read(indexGroup, indexOffset, 8, (void*) &num, throwAdsException, out num2);
            return num;
        }

        public unsafe sbyte ReadInt8(uint indexGroup, uint indexOffset, bool throwAdsException, out AdsErrorCode result)
        {
            sbyte num;
            int num2;
            result = this.Read(indexGroup, indexOffset, 1, (void*) &num, throwAdsException, out num2);
            return num;
        }

        public unsafe float ReadReal32(uint indexGroup, uint indexOffset, bool throwAdsException, out AdsErrorCode result)
        {
            float num;
            int num2;
            result = this.Read(indexGroup, indexOffset, 4, (void*) &num, throwAdsException, out num2);
            return num;
        }

        public unsafe double ReadReal64(uint indexGroup, uint indexOffset, bool throwAdsException, out AdsErrorCode result)
        {
            double num;
            int num2;
            result = this.Read(indexGroup, indexOffset, 8, (void*) &num, throwAdsException, out num2);
            return num;
        }

        [SecuritySafeCritical]
        public virtual unsafe AdsErrorCode ReadState(bool throwAdsException, out StateInfo stateInfo)
        {
            StateInfo info;
            byte[] buffer;
            if (this._disposed)
            {
                throw new ObjectDisposedException(base.GetType().ToString());
            }
            AdsErrorCode noError = AdsErrorCode.NoError;
            if (((buffer = AmsAddressMarshaller.Marshal(this.address)) == null) || (buffer.Length == 0))
            {
                numRef = null;
            }
            else
            {
                numRef = buffer;
            }
            noError = UnsafeNativeMethods.AdsSyncReadStateReq(numRef, &info.adsState, &info.deviceState);
            fixed (byte* numRef = null)
            {
                this.OnHandleCommunicationResult(noError, throwAdsException);
                stateInfo = info;
                return noError;
            }
        }

        public unsafe string ReadString(uint indexGroup, uint indexOffset, int characters, System.Text.Encoding encoding, bool throwAdsException, out AdsErrorCode result)
        {
            string str;
            char[] chars = new char[] { 'a' };
            int byteCount = this.Encoding.GetByteCount(chars);
            int size = characters * byteCount;
            IntPtr ptr = TcAdsDllMarshaller.Alloc(size);
            try
            {
                result = this.Read(indexGroup, indexOffset, size, ptr.ToPointer(), throwAdsException, out size);
                if (result != AdsErrorCode.NoError)
                {
                    str = null;
                }
                else
                {
                    str = TcAdsDllMarshaller.PtrToString(ptr, characters, encoding);
                    int index = str.IndexOf('\0');
                    if (index != -1)
                    {
                        str = str.Substring(0, index);
                    }
                }
            }
            finally
            {
                TcAdsDllMarshaller.Free(ptr);
            }
            return str;
        }

        public unsafe object ReadStruct(uint indexGroup, uint indexOffset, Type structureType, bool throwAdsException, out AdsErrorCode result)
        {
            object obj2;
            int size = 0;
            try
            {
                size = Marshal.SizeOf(structureType);
            }
            catch (Exception exception)
            {
                Module.Trace.TraceError(exception);
                throw;
            }
            IntPtr ptr = TcAdsDllMarshaller.Alloc(size);
            try
            {
                result = this.Read(indexGroup, indexOffset, size, ptr.ToPointer(), throwAdsException, out size);
                obj2 = (result != AdsErrorCode.NoError) ? null : Marshal.PtrToStructure(ptr, structureType);
            }
            finally
            {
                TcAdsDllMarshaller.Free(ptr);
            }
            return obj2;
        }

        public unsafe ushort ReadUInt16(uint indexGroup, uint indexOffset, bool throwAdsException, out AdsErrorCode result)
        {
            ushort num;
            int num2;
            result = this.Read(indexGroup, indexOffset, 2, (void*) &num, throwAdsException, out num2);
            return num;
        }

        public unsafe uint ReadUInt32(uint indexGroup, uint indexOffset, bool throwAdsException, out AdsErrorCode result)
        {
            uint num;
            int num2;
            result = this.Read(indexGroup, indexOffset, 4, (void*) &num, throwAdsException, out num2);
            return num;
        }

        public unsafe ulong ReadUInt64(uint indexGroup, uint indexOffset, bool throwAdsException, out AdsErrorCode result)
        {
            ulong num;
            int num2;
            result = this.Read(indexGroup, indexOffset, 8, (void*) &num, throwAdsException, out num2);
            return num;
        }

        public unsafe byte ReadUInt8(uint indexGroup, uint indexOffset, bool throwAdsException, out AdsErrorCode result)
        {
            byte num;
            int num2;
            result = this.Read(indexGroup, indexOffset, 1, (void*) &num, throwAdsException, out num2);
            return num;
        }

        public unsafe AdsErrorCode ReadWrite(uint indexGroup, uint indexOffset, string wrValue, bool bThrowAdsException, out uint rdValue)
        {
            AdsErrorCode code;
            uint num = 0;
            IntPtr ptr = TcAdsDllMarshaller.StringToPtrAnsi(wrValue);
            try
            {
                int num2;
                code = this.ReadWrite(0xf003, 0, 4, (void*) &num, wrValue.Length + 1, ptr.ToPointer(), false, out num2);
            }
            finally
            {
                TcAdsDllMarshaller.Free(ptr);
            }
            rdValue = num;
            return code;
        }

        [SecuritySafeCritical]
        public virtual unsafe AdsErrorCode ReadWrite(uint indexGroup, uint indexOffset, int readLength, void* readData, int writeLength, void* writeData, bool throwAdsException, out int dataRead)
        {
            int num;
            byte[] buffer;
            AdsErrorCode noError = AdsErrorCode.NoError;
            if (((buffer = AmsAddressMarshaller.Marshal(this.address)) == null) || (buffer.Length == 0))
            {
                numRef = null;
            }
            else
            {
                numRef = buffer;
            }
            noError = UnsafeNativeMethods.AdsSyncReadWriteReqEx(numRef, indexGroup, indexOffset, readLength, readData, writeLength, writeData, &num);
            fixed (byte* numRef = null)
            {
                this.OnHandleCommunicationResult(noError, throwAdsException);
                dataRead = num;
                return noError;
            }
        }

        [SecuritySafeCritical]
        public unsafe AdsErrorCode ReadWrite(uint indexGroup, uint indexOffset, int rdOffset, int rdLength, byte[] rdData, int wrOffset, int wrLength, byte[] wrData, bool throwAdsException, out int dataRead)
        {
            byte[] buffer2;
            ref byte pinned numRef4;
            if ((rdLength != 0) && (wrLength != 0))
            {
                ref byte pinned numRef;
                ref byte pinned numRef2;
                byte[] buffer;
                if (((buffer = rdData) == null) || (buffer.Length == 0))
                {
                    numRef = null;
                }
                else
                {
                    numRef = buffer;
                }
                if (((buffer2 = wrData) == null) || (buffer2.Length == 0))
                {
                    numRef2 = null;
                }
                else
                {
                    numRef2 = buffer2;
                }
                return this.ReadWrite(indexGroup, indexOffset, rdLength, (void*) (numRef + rdOffset), wrLength, (void*) (numRef2 + wrOffset), throwAdsException, out dataRead);
            }
            if ((rdLength != 0) && (wrLength == 0))
            {
                ref byte pinned numRef3;
                if (((buffer2 = rdData) == null) || (buffer2.Length == 0))
                {
                    numRef3 = null;
                }
                else
                {
                    numRef3 = buffer2;
                }
                return this.ReadWrite(indexGroup, indexOffset, rdLength, (void*) (numRef3 + rdOffset), 0, null, throwAdsException, out dataRead);
            }
            if ((rdLength != 0) || (wrLength == 0))
            {
                return this.ReadWrite(indexGroup, indexOffset, 0, null, 0, null, throwAdsException, out dataRead);
            }
            if (((buffer2 = wrData) == null) || (buffer2.Length == 0))
            {
                numRef4 = null;
            }
            else
            {
                numRef4 = buffer2;
            }
            return this.ReadWrite(indexGroup, indexOffset, 0, null, wrLength, (void*) (numRef4 + wrOffset), throwAdsException, out dataRead);
        }

        [SecuritySafeCritical]
        protected virtual AdsErrorCode SetTimeout(int timeout, bool throwAdsException)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(base.GetType().ToString());
            }
            AdsErrorCode adsErrorCode = UnsafeNativeMethods.AdsSyncSetTimeout(timeout);
            if ((adsErrorCode != AdsErrorCode.NoError) & throwAdsException)
            {
                ThrowAdsException(adsErrorCode);
            }
            return adsErrorCode;
        }

        public static void ThrowAdsException(AdsErrorCode adsErrorCode)
        {
            if (adsErrorCode != AdsErrorCode.NoError)
            {
                throw AdsErrorException.Create(adsErrorCode);
            }
        }

        public static void ThrowAdsException(string message, AdsErrorCode adsErrorCode)
        {
            if (adsErrorCode != AdsErrorCode.NoError)
            {
                throw AdsErrorException.Create(message, adsErrorCode);
            }
        }

        public unsafe AdsErrorCode Write(uint indexGroup, uint indexOffset, bool val, bool throwAdsException) => 
            this.Write(indexGroup, indexOffset, 1, (void*) &val, throwAdsException);

        public unsafe AdsErrorCode Write(uint indexGroup, uint indexOffset, byte val, bool throwAdsException) => 
            this.Write(indexGroup, indexOffset, 1, (void*) &val, throwAdsException);

        public unsafe AdsErrorCode Write(uint indexGroup, uint indexOffset, double val, bool throwAdsException) => 
            this.Write(indexGroup, indexOffset, 8, (void*) &val, throwAdsException);

        public unsafe AdsErrorCode Write(uint indexGroup, uint indexOffset, short val, bool throwAdsException) => 
            this.Write(indexGroup, indexOffset, 2, (void*) &val, throwAdsException);

        public unsafe AdsErrorCode Write(uint indexGroup, uint indexOffset, int val, bool throwAdsException) => 
            this.Write(indexGroup, indexOffset, 4, (void*) &val, throwAdsException);

        public unsafe AdsErrorCode Write(uint indexGroup, uint indexOffset, long val, bool throwAdsException) => 
            this.Write(indexGroup, indexOffset, 8, (void*) &val, throwAdsException);

        public unsafe AdsErrorCode Write(uint indexGroup, uint indexOffset, object structure, bool throwAdsException)
        {
            AdsErrorCode code;
            int size = Marshal.SizeOf(structure);
            IntPtr ptr = TcAdsDllMarshaller.Alloc(size);
            try
            {
                Marshal.StructureToPtr(structure, ptr, false);
                code = this.Write(indexGroup, indexOffset, size, ptr.ToPointer(), throwAdsException);
            }
            finally
            {
                TcAdsDllMarshaller.Free(ptr);
            }
            return code;
        }

        public unsafe AdsErrorCode Write(uint indexGroup, uint indexOffset, sbyte val, bool throwAdsException) => 
            this.Write(indexGroup, indexOffset, 1, (void*) &val, throwAdsException);

        public unsafe AdsErrorCode Write(uint indexGroup, uint indexOffset, float val, bool throwAdsException) => 
            this.Write(indexGroup, indexOffset, 4, (void*) &val, throwAdsException);

        public unsafe AdsErrorCode Write(uint indexGroup, uint indexOffset, ushort val, bool throwAdsException) => 
            this.Write(indexGroup, indexOffset, 2, (void*) &val, throwAdsException);

        public unsafe AdsErrorCode Write(uint indexGroup, uint indexOffset, uint val, bool throwAdsException) => 
            this.Write(indexGroup, indexOffset, 4, (void*) &val, throwAdsException);

        public unsafe AdsErrorCode Write(uint indexGroup, uint indexOffset, ulong val, bool throwAdsException) => 
            this.Write(indexGroup, indexOffset, 8, (void*) &val, throwAdsException);

        [SecuritySafeCritical]
        public virtual unsafe AdsErrorCode Write(uint indexGroup, uint indexOffset, int length, void* data, bool throwAdsException)
        {
            byte[] buffer;
            AdsErrorCode noError = AdsErrorCode.NoError;
            if (((buffer = AmsAddressMarshaller.Marshal(this.address)) == null) || (buffer.Length == 0))
            {
                numRef = null;
            }
            else
            {
                numRef = buffer;
            }
            noError = UnsafeNativeMethods.AdsSyncWriteReq(numRef, indexGroup, indexOffset, length, data);
            fixed (byte* numRef = null)
            {
                if ((noError != AdsErrorCode.NoError) & throwAdsException)
                {
                    ThrowAdsException(noError);
                }
                return noError;
            }
        }

        public unsafe AdsErrorCode Write(uint indexGroup, uint indexOffset, string val, int characters, bool throwAdsException)
        {
            AdsErrorCode code;
            if (val.Length > characters)
            {
                val = val.Substring(0, characters);
            }
            IntPtr ptr = TcAdsDllMarshaller.StringToPtrAnsi(val);
            try
            {
                code = this.Write(indexGroup, indexOffset, val.Length + 1, ptr.ToPointer(), throwAdsException);
            }
            finally
            {
                TcAdsDllMarshaller.Free(ptr);
            }
            return code;
        }

        [SecuritySafeCritical]
        public unsafe AdsErrorCode Write(uint indexGroup, uint indexOffset, int offset, int length, byte[] data, bool throwAdsException)
        {
            ref byte pinned numRef;
            byte[] buffer;
            if (length == 0)
            {
                return this.Write(indexGroup, indexOffset, 0, null, throwAdsException);
            }
            if (((buffer = data) == null) || (buffer.Length == 0))
            {
                numRef = null;
            }
            else
            {
                numRef = buffer;
            }
            return this.Write(indexGroup, indexOffset, length, (void*) (numRef + offset), throwAdsException);
        }

        [SecuritySafeCritical]
        protected unsafe AdsErrorCode WriteArrayOfBoolean(uint indexGroup, uint indexOffset, Array arr, bool throwAdsException)
        {
            switch (arr.Rank)
            {
                case 1:
                    ref bool pinned flagRef;
                    bool[] flagArray;
                    if (((flagArray = (bool[]) arr) == null) || (flagArray.Length == 0))
                    {
                        flagRef = null;
                    }
                    else
                    {
                        flagRef = flagArray;
                    }
                    return this.Write(indexGroup, indexOffset, arr.Length, (void*) flagRef, throwAdsException);

                case 2:
                    ref bool pinned flagRef2;
                    bool[,] flagArray2;
                    if (((flagArray2 = (bool[,]) arr) == null) || (flagArray2.Length == 0))
                    {
                        flagRef2 = null;
                    }
                    else
                    {
                        flagRef2 = (bool*) flagArray2[0, 0];
                    }
                    return this.Write(indexGroup, indexOffset, arr.Length, (void*) flagRef2, throwAdsException);

                case 3:
                    ref bool pinned flagRef3;
                    bool[,,] flagArray3;
                    if (((flagArray3 = (bool[,,]) arr) == null) || (flagArray3.Length == 0))
                    {
                        flagRef3 = null;
                    }
                    else
                    {
                        flagRef3 = (bool*) flagArray3[0, 0, 0];
                    }
                    return this.Write(indexGroup, indexOffset, arr.Length, (void*) flagRef3, throwAdsException);
            }
            throw new ArgumentException("Unable to marshal type.");
        }

        [SecuritySafeCritical]
        protected unsafe AdsErrorCode WriteArrayOfInt16(uint indexGroup, uint indexOffset, Array arr, bool throwAdsException)
        {
            switch (arr.Rank)
            {
                case 1:
                    ref short pinned numRef;
                    short[] numArray;
                    if (((numArray = (short[]) arr) == null) || (numArray.Length == 0))
                    {
                        numRef = null;
                    }
                    else
                    {
                        numRef = numArray;
                    }
                    return this.Write(indexGroup, indexOffset, arr.Length * 2, (void*) numRef, throwAdsException);

                case 2:
                    ref short pinned numRef2;
                    short[,] numArray2;
                    if (((numArray2 = (short[,]) arr) == null) || (numArray2.Length == 0))
                    {
                        numRef2 = null;
                    }
                    else
                    {
                        numRef2 = (short*) numArray2[0, 0];
                    }
                    return this.Write(indexGroup, indexOffset, arr.Length * 2, (void*) numRef2, throwAdsException);

                case 3:
                    ref short pinned numRef3;
                    short[,,] numArray3;
                    if (((numArray3 = (short[,,]) arr) == null) || (numArray3.Length == 0))
                    {
                        numRef3 = null;
                    }
                    else
                    {
                        numRef3 = (short*) numArray3[0, 0, 0];
                    }
                    return this.Write(indexGroup, indexOffset, arr.Length * 2, (void*) numRef3, throwAdsException);
            }
            throw new ArgumentException("Unable to marshal type");
        }

        [SecuritySafeCritical]
        protected unsafe AdsErrorCode WriteArrayOfInt32(uint indexGroup, uint indexOffset, Array arr, bool throwAdsException)
        {
            switch (arr.Rank)
            {
                case 1:
                    ref int pinned numRef;
                    int[] numArray;
                    if (((numArray = (int[]) arr) == null) || (numArray.Length == 0))
                    {
                        numRef = null;
                    }
                    else
                    {
                        numRef = numArray;
                    }
                    return this.Write(indexGroup, indexOffset, arr.Length * 4, (void*) numRef, throwAdsException);

                case 2:
                    ref int pinned numRef2;
                    int[,] numArray2;
                    if (((numArray2 = (int[,]) arr) == null) || (numArray2.Length == 0))
                    {
                        numRef2 = null;
                    }
                    else
                    {
                        numRef2 = (int*) numArray2[0, 0];
                    }
                    return this.Write(indexGroup, indexOffset, arr.Length * 4, (void*) numRef2, throwAdsException);

                case 3:
                    ref int pinned numRef3;
                    int[,,] numArray3;
                    if (((numArray3 = (int[,,]) arr) == null) || (numArray3.Length == 0))
                    {
                        numRef3 = null;
                    }
                    else
                    {
                        numRef3 = (int*) numArray3[0, 0, 0];
                    }
                    return this.Write(indexGroup, indexOffset, arr.Length * 4, (void*) numRef3, throwAdsException);
            }
            throw new ArgumentException("Unable to marshal type");
        }

        [SecuritySafeCritical]
        protected unsafe AdsErrorCode WriteArrayOfInt64(uint indexGroup, uint indexOffset, Array arr, bool throwAdsException)
        {
            switch (arr.Rank)
            {
                case 1:
                    ref long pinned numRef;
                    long[] numArray;
                    if (((numArray = (long[]) arr) == null) || (numArray.Length == 0))
                    {
                        numRef = null;
                    }
                    else
                    {
                        numRef = numArray;
                    }
                    return this.Write(indexGroup, indexOffset, arr.Length * 8, (void*) numRef, throwAdsException);

                case 2:
                    ref long pinned numRef2;
                    long[,] numArray2;
                    if (((numArray2 = (long[,]) arr) == null) || (numArray2.Length == 0))
                    {
                        numRef2 = null;
                    }
                    else
                    {
                        numRef2 = (long*) numArray2[0, 0];
                    }
                    return this.Write(indexGroup, indexOffset, arr.Length * 8, (void*) numRef2, throwAdsException);

                case 3:
                    ref long pinned numRef3;
                    long[,,] numArray3;
                    if (((numArray3 = (long[,,]) arr) == null) || (numArray3.Length == 0))
                    {
                        numRef3 = null;
                    }
                    else
                    {
                        numRef3 = (long*) numArray3[0, 0, 0];
                    }
                    return this.Write(indexGroup, indexOffset, arr.Length * 8, (void*) numRef3, throwAdsException);
            }
            throw new ArgumentException("Unable to marshal type");
        }

        [SecuritySafeCritical]
        protected unsafe AdsErrorCode WriteArrayOfInt8(uint indexGroup, uint indexOffset, Array arr, bool throwAdsException)
        {
            switch (arr.Rank)
            {
                case 1:
                    ref sbyte pinned numRef;
                    sbyte[] numArray;
                    if (((numArray = (sbyte[]) arr) == null) || (numArray.Length == 0))
                    {
                        numRef = null;
                    }
                    else
                    {
                        numRef = numArray;
                    }
                    return this.Write(indexGroup, indexOffset, arr.Length, (void*) numRef, throwAdsException);

                case 2:
                    ref sbyte pinned numRef2;
                    sbyte[,] numArray2;
                    if (((numArray2 = (sbyte[,]) arr) == null) || (numArray2.Length == 0))
                    {
                        numRef2 = null;
                    }
                    else
                    {
                        numRef2 = (sbyte*) numArray2[0, 0];
                    }
                    return this.Write(indexGroup, indexOffset, arr.Length, (void*) numRef2, throwAdsException);

                case 3:
                    ref sbyte pinned numRef3;
                    sbyte[,,] numArray3;
                    if (((numArray3 = (sbyte[,,]) arr) == null) || (numArray3.Length == 0))
                    {
                        numRef3 = null;
                    }
                    else
                    {
                        numRef3 = (sbyte*) numArray3[0, 0, 0];
                    }
                    return this.Write(indexGroup, indexOffset, arr.Length, (void*) numRef3, throwAdsException);
            }
            throw new ArgumentException("Unable to marshal type");
        }

        [SecuritySafeCritical]
        protected unsafe AdsErrorCode WriteArrayOfReal32(uint indexGroup, uint indexOffset, Array arr, bool throwAdsException)
        {
            switch (arr.Rank)
            {
                case 1:
                    ref float pinned numRef;
                    float[] numArray;
                    if (((numArray = (float[]) arr) == null) || (numArray.Length == 0))
                    {
                        numRef = null;
                    }
                    else
                    {
                        numRef = numArray;
                    }
                    return this.Write(indexGroup, indexOffset, arr.Length * 4, (void*) numRef, throwAdsException);

                case 2:
                    ref float pinned numRef2;
                    float[,] numArray2;
                    if (((numArray2 = (float[,]) arr) == null) || (numArray2.Length == 0))
                    {
                        numRef2 = null;
                    }
                    else
                    {
                        numRef2 = (float*) numArray2[0, 0];
                    }
                    return this.Write(indexGroup, indexOffset, arr.Length * 4, (void*) numRef2, throwAdsException);

                case 3:
                    ref float pinned numRef3;
                    float[,,] numArray3;
                    if (((numArray3 = (float[,,]) arr) == null) || (numArray3.Length == 0))
                    {
                        numRef3 = null;
                    }
                    else
                    {
                        numRef3 = (float*) numArray3[0, 0, 0];
                    }
                    return this.Write(indexGroup, indexOffset, arr.Length * 4, (void*) numRef3, throwAdsException);
            }
            throw new ArgumentException("Unable to marshal type");
        }

        [SecuritySafeCritical]
        protected unsafe AdsErrorCode WriteArrayOfReal64(uint indexGroup, uint indexOffset, Array arr, bool throwAdsException)
        {
            switch (arr.Rank)
            {
                case 1:
                    ref double pinned numRef;
                    double[] numArray;
                    if (((numArray = (double[]) arr) == null) || (numArray.Length == 0))
                    {
                        numRef = null;
                    }
                    else
                    {
                        numRef = numArray;
                    }
                    return this.Write(indexGroup, indexOffset, arr.Length * 8, (void*) numRef, throwAdsException);

                case 2:
                    ref double pinned numRef2;
                    double[,] numArray2;
                    if (((numArray2 = (double[,]) arr) == null) || (numArray2.Length == 0))
                    {
                        numRef2 = null;
                    }
                    else
                    {
                        numRef2 = (double*) numArray2[0, 0];
                    }
                    return this.Write(indexGroup, indexOffset, arr.Length * 8, (void*) numRef2, throwAdsException);

                case 3:
                    ref double pinned numRef3;
                    double[,,] numArray3;
                    if (((numArray3 = (double[,,]) arr) == null) || (numArray3.Length == 0))
                    {
                        numRef3 = null;
                    }
                    else
                    {
                        numRef3 = (double*) numArray3[0, 0, 0];
                    }
                    return this.Write(indexGroup, indexOffset, arr.Length * 8, (void*) numRef3, throwAdsException);
            }
            throw new ArgumentException("Unable to marshal type");
        }

        protected unsafe AdsErrorCode WriteArrayOfString(uint indexGroup, uint indexOffset, string[] val, int characters, bool throwAdsException)
        {
            AdsErrorCode code;
            int size = (characters + 1) * val.Length;
            byte* numPtr = (byte*) Memory.Alloc(size);
            for (int i = 0; i < val.Length; i++)
            {
                IntPtr ptr = TcAdsDllMarshaller.StringToPtrAnsi(val[i]);
                try
                {
                    Memory.Copy(ptr.ToPointer(), (void*) (numPtr + (i * (characters + 1))), val[i].Length + 1);
                }
                finally
                {
                    TcAdsDllMarshaller.Free(ptr);
                }
            }
            try
            {
                code = this.Write(indexGroup, indexOffset, size, (void*) numPtr, throwAdsException);
            }
            finally
            {
                Memory.Free((void*) numPtr);
            }
            return code;
        }

        protected unsafe AdsErrorCode WriteArrayOfStruct(uint indexGroup, uint indexOffset, object val, bool throwAdsException)
        {
            AdsErrorCode code;
            Array array = (Array) val;
            int elementSize = Marshal.SizeOf(array.GetValue(0).GetType());
            int size = elementSize * array.GetLength(0);
            IntPtr ptr = TcAdsDllMarshaller.Alloc(size);
            try
            {
                int index = 0;
                while (true)
                {
                    if (index >= array.GetLength(0))
                    {
                        code = this.Write(indexGroup, indexOffset, size, ptr.ToPointer(), throwAdsException);
                        break;
                    }
                    Marshal.StructureToPtr(array.GetValue(index), this.CreateArrayElementIntPtr(ptr, index, elementSize), false);
                    index++;
                }
            }
            finally
            {
                TcAdsDllMarshaller.Free(ptr);
            }
            return code;
        }

        [SecuritySafeCritical]
        protected unsafe AdsErrorCode WriteArrayOfUInt16(uint indexGroup, uint indexOffset, Array arr, bool throwAdsException)
        {
            switch (arr.Rank)
            {
                case 1:
                    ref ushort pinned numRef;
                    ushort[] numArray;
                    if (((numArray = (ushort[]) arr) == null) || (numArray.Length == 0))
                    {
                        numRef = null;
                    }
                    else
                    {
                        numRef = numArray;
                    }
                    return this.Write(indexGroup, indexOffset, arr.Length * 2, (void*) numRef, throwAdsException);

                case 2:
                    ref ushort pinned numRef2;
                    ushort[,] numArray2;
                    if (((numArray2 = (ushort[,]) arr) == null) || (numArray2.Length == 0))
                    {
                        numRef2 = null;
                    }
                    else
                    {
                        numRef2 = (ushort*) numArray2[0, 0];
                    }
                    return this.Write(indexGroup, indexOffset, arr.Length * 2, (void*) numRef2, throwAdsException);

                case 3:
                    ref ushort pinned numRef3;
                    ushort[,,] numArray3;
                    if (((numArray3 = (ushort[,,]) arr) == null) || (numArray3.Length == 0))
                    {
                        numRef3 = null;
                    }
                    else
                    {
                        numRef3 = (ushort*) numArray3[0, 0, 0];
                    }
                    return this.Write(indexGroup, indexOffset, arr.Length * 2, (void*) numRef3, throwAdsException);
            }
            throw new ArgumentException("Unable to marshal type");
        }

        [SecuritySafeCritical]
        protected unsafe AdsErrorCode WriteArrayOfUInt32(uint indexGroup, uint indexOffset, Array arr, bool throwAdsException)
        {
            switch (arr.Rank)
            {
                case 1:
                    ref uint pinned numRef;
                    uint[] numArray;
                    if (((numArray = (uint[]) arr) == null) || (numArray.Length == 0))
                    {
                        numRef = null;
                    }
                    else
                    {
                        numRef = numArray;
                    }
                    return this.Write(indexGroup, indexOffset, arr.Length * 4, (void*) numRef, throwAdsException);

                case 2:
                    ref uint pinned numRef2;
                    uint[,] numArray2;
                    if (((numArray2 = (uint[,]) arr) == null) || (numArray2.Length == 0))
                    {
                        numRef2 = null;
                    }
                    else
                    {
                        numRef2 = (uint*) numArray2[0, 0];
                    }
                    return this.Write(indexGroup, indexOffset, arr.Length * 4, (void*) numRef2, throwAdsException);

                case 3:
                    ref uint pinned numRef3;
                    uint[,,] numArray3;
                    if (((numArray3 = (uint[,,]) arr) == null) || (numArray3.Length == 0))
                    {
                        numRef3 = null;
                    }
                    else
                    {
                        numRef3 = (uint*) numArray3[0, 0, 0];
                    }
                    return this.Write(indexGroup, indexOffset, arr.Length * 4, (void*) numRef3, throwAdsException);
            }
            throw new ArgumentException("Unable to marshal type");
        }

        [SecuritySafeCritical]
        protected unsafe AdsErrorCode WriteArrayOfUInt8(uint indexGroup, uint indexOffset, Array arr, bool throwAdsException)
        {
            switch (arr.Rank)
            {
                case 1:
                    ref byte pinned numRef;
                    byte[] buffer;
                    if (((buffer = (byte[]) arr) == null) || (buffer.Length == 0))
                    {
                        numRef = null;
                    }
                    else
                    {
                        numRef = buffer;
                    }
                    return this.Write(indexGroup, indexOffset, arr.Length, (void*) numRef, throwAdsException);

                case 2:
                    ref byte pinned numRef2;
                    byte[,] buffer2;
                    if (((buffer2 = (byte[,]) arr) == null) || (buffer2.Length == 0))
                    {
                        numRef2 = null;
                    }
                    else
                    {
                        numRef2 = (byte*) buffer2[0, 0];
                    }
                    return this.Write(indexGroup, indexOffset, arr.Length, (void*) numRef2, throwAdsException);

                case 3:
                    ref byte pinned numRef3;
                    byte[,,] buffer3;
                    if (((buffer3 = (byte[,,]) arr) == null) || (buffer3.Length == 0))
                    {
                        numRef3 = null;
                    }
                    else
                    {
                        numRef3 = (byte*) buffer3[0, 0, 0];
                    }
                    return this.Write(indexGroup, indexOffset, arr.Length, (void*) numRef3, throwAdsException);
            }
            throw new ArgumentException("Unable to marshal type");
        }

        [SecuritySafeCritical]
        protected virtual unsafe AdsErrorCode WriteControl(AdsState adsState, short deviceState, int length, void* data, bool throwAdsException)
        {
            byte[] buffer;
            AdsErrorCode noError = AdsErrorCode.NoError;
            if (((buffer = AmsAddressMarshaller.Marshal(this.address)) == null) || (buffer.Length == 0))
            {
                numRef = null;
            }
            else
            {
                numRef = buffer;
            }
            noError = UnsafeNativeMethods.AdsSyncWriteControlReq(numRef, adsState, deviceState, length, data);
            fixed (byte* numRef = null)
            {
                this.OnHandleCommunicationResult(noError, throwAdsException);
                return noError;
            }
        }

        [SecuritySafeCritical]
        public unsafe AdsErrorCode WriteControl(StateInfo stateInfo, byte[] data, int offset, int length, bool throwAdsException)
        {
            ref byte pinned numRef;
            byte[] buffer;
            if (((buffer = data) == null) || (buffer.Length == 0))
            {
                numRef = null;
            }
            else
            {
                numRef = buffer;
            }
            return this.WriteControl(stateInfo.AdsState, stateInfo.DeviceState, length, (void*) (numRef + offset), throwAdsException);
        }

        public bool IsDisposed =>
            this._disposed;

        public AmsAddress TargetAddress =>
            this.address;

        public int Timeout
        {
            get
            {
                int num;
                this.GetTimeout(true, out num);
                return num;
            }
            set => 
                this.SetTimeout(value, true);
        }

        internal System.Text.Encoding Encoding
        {
            get => 
                this._encoding;
            set => 
                (this._encoding = value);
        }

        internal delegate void AdsNotificationDelegate(IntPtr pAddr, IntPtr pNotification, int hUser);

        internal delegate void AmsRouterNotificationDelegate(AmsRouterState state);

        internal class UnsafeNativeMethods
        {
            [DllImport("tcadsdll.dll")]
            public static extern unsafe AdsErrorCode AdsAmsPortEnabled(int* pbEnabled);
            [DllImport("tcadsdll.dll")]
            public static extern unsafe AdsErrorCode AdsAmsPortEnabledEx(int port, int* pbEnabled);
            [DllImport("tcadsdll.dll")]
            public static extern AdsErrorCode AdsAmsRegisterRouterNotification(TcAdsDllWrapper.AmsRouterNotificationDelegate pNoteFunc);
            [DllImport("tcadsdll.dll")]
            public static extern AdsErrorCode AdsAmsUnRegisterRouterNotification();
            [DllImport("tcadsdll.dll")]
            public static extern int AdsGetDllVersion();
            [DllImport("tcadsdll.dll")]
            public static extern unsafe AdsErrorCode AdsGetLocalAddress(byte* data);
            [DllImport("tcadsdll.dll")]
            public static extern unsafe AdsErrorCode AdsGetLocalAddressEx(int port, void* data);
            [DllImport("tcadsdll.dll")]
            public static extern AdsErrorCode AdsPortClose();
            [DllImport("tcadsdll.dll")]
            public static extern AdsErrorCode AdsPortCloseEx(int port);
            [DllImport("tcadsdll.dll")]
            public static extern int AdsPortOpen();
            [DllImport("tcadsdll.dll")]
            public static extern int AdsPortOpenEx();
            [DllImport("tcadsdll.dll")]
            public static extern unsafe AdsErrorCode AdsSyncAddDeviceNotificationReq(byte* pAddr, uint indexGroup, uint indexOffset, AdsNotificationAttrib* pNoteAttrib, IntPtr pNoteFunc, int hUser, int* pNotification);
            [DllImport("tcadsdll.dll")]
            public static extern unsafe AdsErrorCode AdsSyncAddDeviceNotificationReqEx(int port, byte* pAddr, uint indexGroup, uint indexOffset, AdsNotificationAttrib* pNoteAttrib, IntPtr pNoteFunc, int hUser, int* pNotification);
            [DllImport("tcadsdll.dll")]
            public static extern unsafe AdsErrorCode AdsSyncDelDeviceNotificationReq(byte* pAddr, int hNotification);
            [DllImport("tcadsdll.dll")]
            public static extern unsafe AdsErrorCode AdsSyncDelDeviceNotificationReqEx(int port, byte* pAddr, int hNotification);
            [DllImport("tcadsdll.dll")]
            public static extern unsafe AdsErrorCode AdsSyncGetTimeout(int* pTimeout);
            [DllImport("tcadsdll.dll")]
            public static extern unsafe AdsErrorCode AdsSyncGetTimeoutEx(int port, int* pTimeout);
            [DllImport("tcadsdll.dll")]
            public static extern unsafe AdsErrorCode AdsSyncReadDeviceInfoReq(byte* pAddr, byte* pDevName, byte* pVersion);
            [DllImport("tcadsdll.dll")]
            public static extern unsafe AdsErrorCode AdsSyncReadDeviceInfoReqEx(int port, byte* pAddr, byte* pDevName, byte* pVersion);
            [DllImport("tcadsdll.dll")]
            public static extern unsafe AdsErrorCode AdsSyncReadReq(byte* addr, uint indexGroup, uint indexOffset, int length, void* data);
            [DllImport("tcadsdll.dll")]
            public static extern unsafe AdsErrorCode AdsSyncReadReqEx(byte* addr, uint indexGroup, uint indexOffset, int length, void* data, int* pcbReturn);
            [DllImport("tcadsdll.dll")]
            public static extern unsafe AdsErrorCode AdsSyncReadReqEx2(int port, byte* addr, uint indexGroup, uint indexOffset, int length, void* data, int* pcbReturn);
            [DllImport("tcadsdll.dll")]
            public static extern unsafe AdsErrorCode AdsSyncReadStateReq(byte* pAddr, AdsState* pAdsState, short* pDeviceState);
            [DllImport("tcadsdll.dll")]
            public static extern unsafe AdsErrorCode AdsSyncReadStateReqEx(int port, byte* pAddr, AdsState* pAdsState, short* pDeviceState);
            [DllImport("tcadsdll.dll")]
            public static extern unsafe AdsErrorCode AdsSyncReadWriteReq(byte* pServerAddr, uint indexGroup, uint indexOffset, int cbReadLength, void* pReadData, int cbWriteLength, void* pWriteData);
            [DllImport("tcadsdll.dll")]
            public static extern unsafe AdsErrorCode AdsSyncReadWriteReqEx(byte* pServerAddr, uint indexGroup, uint indexOffset, int cbReadLength, void* pReadData, int cbWriteLength, void* pWriteData, int* pcbReturn);
            [DllImport("tcadsdll.dll")]
            public static extern unsafe AdsErrorCode AdsSyncReadWriteReqEx2(int port, byte* pAddr, uint indexGroup, uint indexOffset, int cbReadLength, void* pReadData, int cbWriteLength, void* pWriteData, int* pcbReturn);
            [DllImport("tcadsdll.dll")]
            public static extern AdsErrorCode AdsSyncSetTimeout(int timeout);
            [DllImport("tcadsdll.dll")]
            public static extern AdsErrorCode AdsSyncSetTimeoutEx(int port, int timeout);
            [DllImport("tcadsdll.dll")]
            public static extern unsafe AdsErrorCode AdsSyncWriteControlReq(byte* pAddr, AdsState adsState, short deviceState, int length, void* pData);
            [DllImport("tcadsdll.dll")]
            public static extern unsafe AdsErrorCode AdsSyncWriteControlReqEx(int port, byte* pAddr, AdsState adsState, short deviceState, int length, void* pData);
            [DllImport("tcadsdll.dll")]
            public static extern unsafe AdsErrorCode AdsSyncWriteReq(byte* pServerAddr, uint indexGroup, uint indexOffset, int length, void* pData);
            [DllImport("tcadsdll.dll")]
            public static extern unsafe AdsErrorCode AdsSyncWriteReqEx(int port, byte* pAddr, uint indexGroup, uint indexOffset, int length, void* pData);
        }
    }
}

