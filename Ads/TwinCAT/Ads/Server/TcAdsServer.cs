namespace TwinCAT.Ads.Server
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using TwinCAT.Ads;

    [ComVisible(false)]
    public class TcAdsServer : IDisposable
    {
        private TcAmsServer _amsServer;
        private bool _isConnected;
        private bool _useSingleNotificationEventHandler;
        [CompilerGenerated]
        private TcAdsServerExDelegate _adsServerExEvent;
        private bool _disposed;

        private event TcAdsServerExDelegate _adsServerExEvent
        {
            [CompilerGenerated] add
            {
                TcAdsServerExDelegate objA = this._adsServerExEvent;
                while (true)
                {
                    TcAdsServerExDelegate a = objA;
                    TcAdsServerExDelegate delegate4 = (TcAdsServerExDelegate) Delegate.Combine(a, value);
                    objA = Interlocked.CompareExchange<TcAdsServerExDelegate>(ref this._adsServerExEvent, delegate4, a);
                    if (ReferenceEquals(objA, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                TcAdsServerExDelegate objA = this._adsServerExEvent;
                while (true)
                {
                    TcAdsServerExDelegate source = objA;
                    TcAdsServerExDelegate delegate4 = (TcAdsServerExDelegate) Delegate.Remove(source, value);
                    objA = Interlocked.CompareExchange<TcAdsServerExDelegate>(ref this._adsServerExEvent, delegate4, source);
                    if (ReferenceEquals(objA, source))
                    {
                        return;
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        public event TcAdsServerExDelegate AdsServerException
        {
            add
            {
                this._adsServerExEvent += value;
            }
            remove
            {
                this._adsServerExEvent -= value;
            }
        }

        public TcAdsServer(string portName)
        {
            this._amsServer = new TcAmsServer(portName);
        }

        public TcAdsServer(ushort port, string portName)
        {
            this._amsServer = new TcAmsServer(port, portName);
        }

        public TcAdsServer(ushort port, string portName, bool useSingleNotificationHandler) : this(port, portName)
        {
            this._useSingleNotificationEventHandler = useSingleNotificationHandler;
        }

        [ComVisible(false)]
        public virtual void AdsAddDeviceNotificationCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result, uint notificationHandle)
        {
        }

        [ComVisible(false)]
        public virtual void AdsAddDeviceNotificationInd(AmsAddress rAddr, uint invokeId, uint indexGroup, uint indexOffset, uint cbLength, AdsTransMode transMode, uint maxDelay, uint cycleTime)
        {
            this.AdsAddDeviceNotificationRes(rAddr, invokeId, AdsErrorCode.DeviceServiceNotSupported, 0);
        }

        [ComVisible(false)]
        public AdsErrorCode AdsAddDeviceNotificationReq(AmsAddress rAddr, uint invokeId, uint indexGroup, uint indexOffset, uint cbLength, AdsTransMode transMode, uint maxDelay, uint cycleTime)
        {
            TcAdsAddDeviceNotificationReqHeader adsHeader = new TcAdsAddDeviceNotificationReqHeader {
                _indexGroup = indexGroup,
                _indexOffset = indexOffset,
                _cbLength = cbLength,
                _transMode = transMode,
                _maxDelay = maxDelay,
                _cycleTime = cycleTime
            };
            return this.AdsRequest(rAddr, invokeId, 6, 0, adsHeader, null);
        }

        [ComVisible(false)]
        public AdsErrorCode AdsAddDeviceNotificationRes(AmsAddress rAddr, uint invokeId, AdsErrorCode result, uint handle)
        {
            TcAdsAddDeviceNotificationResHeader adsHeader = new TcAdsAddDeviceNotificationResHeader {
                _result = result,
                _notificationHandle = handle
            };
            return this.AdsResponse(rAddr, invokeId, 6, 0, adsHeader, null);
        }

        [ComVisible(false)]
        public virtual void AdsDelDeviceNotificationCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result)
        {
        }

        [ComVisible(false)]
        public virtual void AdsDelDeviceNotificationInd(AmsAddress rAddr, uint invokeId, uint hNotification)
        {
            this.AdsDelDeviceNotificationRes(rAddr, invokeId, AdsErrorCode.DeviceServiceNotSupported);
        }

        [ComVisible(false)]
        public AdsErrorCode AdsDelDeviceNotificationReq(AmsAddress rAddr, uint invokeId, uint hNotification)
        {
            TcAdsDelDeviceNotificationReqHeader adsHeader = new TcAdsDelDeviceNotificationReqHeader {
                _notificationHandle = hNotification
            };
            return this.AdsRequest(rAddr, invokeId, 7, 0, adsHeader, null);
        }

        [ComVisible(false)]
        public AdsErrorCode AdsDelDeviceNotificationRes(AmsAddress rAddr, uint invokeId, AdsErrorCode result)
        {
            TcAdsDelDeviceNotificationResHeader adsHeader = new TcAdsDelDeviceNotificationResHeader {
                _result = result
            };
            return this.AdsResponse(rAddr, invokeId, 7, 0, adsHeader, null);
        }

        [ComVisible(false)]
        public virtual void AdsDeviceNotificationInd(AmsAddress rAddr, uint invokeId, uint numStapHeaders, TcAdsStampHeader[] stampHeaders)
        {
        }

        [ComVisible(false)]
        public virtual void AdsDeviceNotificationInd(AmsAddress rAddr, uint invokeId, uint numStampHeaders, AdsBinaryReader stampReader)
        {
        }

        [ComVisible(false)]
        public AdsErrorCode AdsDeviceNotificationReq(AmsAddress rAddr, uint invokeId, uint numStampHeaders, TcAdsStampHeader[] notificationHeaders)
        {
            TcAdsDeviceNotificationReqHeader adsHeader = new TcAdsDeviceNotificationReqHeader {
                _numStamps = numStampHeaders
            };
            byte[] adsData = TcAdsParser.BuildDeviceNotificationBuffer(notificationHeaders);
            adsHeader._cbData = (uint) adsData.Length;
            return this.AdsRequest(rAddr, invokeId, 8, (uint) adsData.Length, adsHeader, adsData);
        }

        [ComVisible(false)]
        public virtual void AdsReadCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result, uint cbLength, byte[] data)
        {
        }

        [ComVisible(false)]
        public virtual void AdsReadDeviceInfoCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result, string name, AdsVersion version)
        {
        }

        [ComVisible(false)]
        public virtual void AdsReadDeviceInfoInd(AmsAddress rAddr, uint invokeId)
        {
            AdsVersion version = new AdsVersion();
            this.AdsReadDeviceInfoRes(rAddr, invokeId, AdsErrorCode.DeviceServiceNotSupported, "", version);
        }

        [ComVisible(false)]
        public AdsErrorCode AdsReadDeviceInfoReq(AmsAddress rAddr, uint invokeId) => 
            this.AdsRequest(rAddr, invokeId, 1, 0, null, null);

        [ComVisible(false)]
        public AdsErrorCode AdsReadDeviceInfoRes(AmsAddress rAddr, uint invokeId, AdsErrorCode result, string name, AdsVersion version)
        {
            TcAdsReadDeviceInfoResHeader adsHeader = new TcAdsReadDeviceInfoResHeader {
                _result = result,
                _majorVersion = version.Version,
                _minorVersion = version.Revision,
                _versionBuild = (ushort) version.Build,
                _deviceName = Encoding.ASCII.GetBytes(name.ToCharArray(), 0, (name.Length < 0x10) ? name.Length : 0x10)
            };
            return this.AdsResponse(rAddr, invokeId, 1, 0, adsHeader, null);
        }

        [ComVisible(false)]
        public virtual void AdsReadInd(AmsAddress rAddr, uint invokeId, uint indexGroup, uint indexOffset, uint cbLength)
        {
            this.AdsReadRes(rAddr, invokeId, AdsErrorCode.DeviceServiceNotSupported, 0, null);
        }

        [ComVisible(false)]
        public AdsErrorCode AdsReadReq(AmsAddress rAddr, uint invokeId, uint indexGroup, uint indexOffset, uint cbLength)
        {
            TcAdsReadReqHeader adsHeader = new TcAdsReadReqHeader {
                _indexGroup = indexGroup,
                _indexOffset = indexOffset,
                _cbLength = cbLength
            };
            return this.AdsRequest(rAddr, invokeId, 2, 0, adsHeader, null);
        }

        [ComVisible(false)]
        public AdsErrorCode AdsReadRes(AmsAddress rAddr, uint invokeId, AdsErrorCode result, uint cbLength, byte[] data)
        {
            TcAdsReadResHeader adsHeader = new TcAdsReadResHeader {
                _result = result,
                _cbData = cbLength
            };
            uint length = 0;
            if (data != null)
            {
                length = (uint) data.Length;
            }
            return this.AdsResponse(rAddr, invokeId, 2, length, adsHeader, data);
        }

        [ComVisible(false)]
        public virtual void AdsReadStateCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result, AdsState adsState, ushort deviceState)
        {
        }

        [ComVisible(false)]
        public virtual void AdsReadStateInd(AmsAddress rAddr, uint invokeId)
        {
            this.AdsReadStateRes(rAddr, invokeId, AdsErrorCode.DeviceServiceNotSupported, AdsState.Invalid, 0);
        }

        [ComVisible(false)]
        public AdsErrorCode AdsReadStateReq(AmsAddress rAddr, uint invokeId) => 
            this.AdsRequest(rAddr, invokeId, 4, 0, null, null);

        [ComVisible(false)]
        public AdsErrorCode AdsReadStateRes(AmsAddress rAddr, uint invokeId, AdsErrorCode result, AdsState adsState, ushort deviceState)
        {
            TcAdsReadStateResHeader adsHeader = new TcAdsReadStateResHeader {
                _result = result,
                _adsState = (ushort) adsState,
                _deviceState = deviceState
            };
            return this.AdsResponse(rAddr, invokeId, 4, 0, adsHeader, null);
        }

        [ComVisible(false)]
        public virtual void AdsReadWriteCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result, uint cbLength, byte[] data)
        {
        }

        [ComVisible(false)]
        public virtual void AdsReadWriteInd(AmsAddress rAddr, uint invokeId, uint indexGroup, uint indexOffset, uint cbReadLength, uint cbWriteLength, byte[] data)
        {
            this.AdsReadWriteRes(rAddr, invokeId, AdsErrorCode.DeviceServiceNotSupported, 0, null);
        }

        [ComVisible(false)]
        public AdsErrorCode AdsReadWriteReq(AmsAddress rAddr, uint invokeId, uint indexGroup, uint indexOffset, uint cbReadLength, uint cbWriteLength, byte[] data)
        {
            TcAdsReadWriteReqHeader adsHeader = new TcAdsReadWriteReqHeader {
                _indexGroup = indexGroup,
                _indexOffset = indexOffset,
                _cbReadLength = cbReadLength,
                _cbWriteLength = cbWriteLength
            };
            uint cbLength = 0;
            if (data != null)
            {
                cbLength = (uint) data.Length;
            }
            return this.AdsRequest(rAddr, invokeId, 9, cbLength, adsHeader, data);
        }

        [ComVisible(false)]
        public AdsErrorCode AdsReadWriteRes(AmsAddress rAddr, uint invokeId, AdsErrorCode result, uint cbLength, byte[] data)
        {
            TcAdsReadWriteResHeader adsHeader = new TcAdsReadWriteResHeader {
                _result = result,
                _cbData = cbLength
            };
            uint length = 0;
            if (data != null)
            {
                length = (uint) data.Length;
            }
            return this.AdsResponse(rAddr, invokeId, 9, length, adsHeader, data);
        }

        private AdsErrorCode AdsRequest(AmsAddress rAddr, uint invokeId, ushort serviceId, uint cbLength, ITcAdsHeader adsHeader, byte[] adsData)
        {
            uint num = 0;
            if (adsHeader != null)
            {
                num = (uint) Marshal.SizeOf(adsHeader);
            }
            TcAmsHeader amsHeader = new TcAmsHeader(rAddr, this.Address, serviceId, 4, num + cbLength, 0, invokeId);
            return this._amsServer.Send(new TcAmsCommand(amsHeader, TcAdsParser.BuildAdsBuffer(adsHeader, adsData)));
        }

        private AdsErrorCode AdsResponse(AmsAddress rAddr, uint invokeId, ushort serviceId, uint cbLength, ITcAdsHeader adsHeader, byte[] adsData)
        {
            TcAmsHeader amsHeader = new TcAmsHeader(rAddr, this.Address, serviceId, 5, ((uint) Marshal.SizeOf(adsHeader)) + cbLength, 0, invokeId);
            return this._amsServer.Send(new TcAmsCommand(amsHeader, TcAdsParser.BuildAdsBuffer(adsHeader, adsData)));
        }

        [ComVisible(false)]
        public virtual void AdsWriteCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result)
        {
        }

        [ComVisible(false)]
        public virtual void AdsWriteControlCon(AmsAddress rAddr, uint invokeId, AdsErrorCode result)
        {
        }

        [ComVisible(false)]
        public virtual void AdsWriteControlInd(AmsAddress rAddr, uint invokeId, AdsState adsState, ushort deviceState, uint cbLength, byte[] pDeviceData)
        {
            this.AdsWriteControlRes(rAddr, invokeId, AdsErrorCode.DeviceServiceNotSupported);
        }

        [ComVisible(false)]
        public AdsErrorCode AdsWriteControlReq(AmsAddress rAddr, uint invokeId, AdsState adsState, ushort deviceState, uint cbLength, byte[] data)
        {
            TcAdsWriteControlReqHeader adsHeader = new TcAdsWriteControlReqHeader {
                _adsState = adsState,
                _deviceState = deviceState,
                _cbLength = cbLength
            };
            uint length = 0;
            if (data != null)
            {
                length = (uint) data.Length;
            }
            return this.AdsRequest(rAddr, invokeId, 5, length, adsHeader, data);
        }

        [ComVisible(false)]
        public AdsErrorCode AdsWriteControlRes(AmsAddress rAddr, uint invokeId, AdsErrorCode result)
        {
            TcAdsWriteControlResHeader adsHeader = new TcAdsWriteControlResHeader {
                _result = result
            };
            return this.AdsResponse(rAddr, invokeId, 5, 0, adsHeader, null);
        }

        [ComVisible(false)]
        public virtual void AdsWriteInd(AmsAddress rAddr, uint invokeId, uint indexGroup, uint indexOffset, uint cbLength, byte[] data)
        {
            this.AdsWriteRes(rAddr, invokeId, AdsErrorCode.DeviceServiceNotSupported);
        }

        [ComVisible(false)]
        public AdsErrorCode AdsWriteReq(AmsAddress rAddr, uint invokeId, uint indexGroup, uint indexOffset, uint cbLength, byte[] data)
        {
            TcAdsWriteReqHeader adsHeader = new TcAdsWriteReqHeader {
                _indexGroup = indexGroup,
                _indexOffset = indexOffset,
                _cbLength = cbLength
            };
            uint length = 0;
            if (data != null)
            {
                length = (uint) data.Length;
            }
            return this.AdsRequest(rAddr, invokeId, 3, length, adsHeader, data);
        }

        [ComVisible(false)]
        public AdsErrorCode AdsWriteRes(AmsAddress rAddr, uint invokeId, AdsErrorCode result)
        {
            TcAdsWriteResHeader adsHeader = new TcAdsWriteResHeader {
                _result = result
            };
            return this.AdsResponse(rAddr, invokeId, 3, 0, adsHeader, null);
        }

        public void Connect()
        {
            try
            {
                this._amsServer.AmsServerException += new TcAmsServerMarshaller.TcAmsServerExEventDelegate(this.OnAmsServerException);
                this._amsServer.Connect();
                this._amsServer.OnAmsReceive += new TcAmsServer.TcAmsServerReceiveEventHandler(this.OnAmsReceive);
                this._isConnected = true;
            }
            catch (TcAmsServerException exception)
            {
                throw new TcAdsServerException(exception.Message, exception.ErrorCode);
            }
        }

        public void Disconnect()
        {
            try
            {
                this._isConnected = false;
                this._amsServer.OnAmsReceive -= new TcAmsServer.TcAmsServerReceiveEventHandler(this.OnAmsReceive);
                this._amsServer.Disconnect();
                this._amsServer.AmsServerException -= new TcAmsServerMarshaller.TcAmsServerExEventDelegate(this.OnAmsServerException);
            }
            catch (TcAmsServerException exception)
            {
                throw new TcAdsServerException(exception.Message, exception.ErrorCode);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            this._disposed = true;
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && this._isConnected)
            {
                this.Disconnect();
            }
        }

        ~TcAdsServer()
        {
            this.Dispose(false);
        }

        private void OnAmsReceive(TcAmsCommand amsCmd)
        {
            byte[] data = amsCmd.Data;
            uint indexGroup = 0;
            uint cbLength = 0;
            uint cbWriteLength = 0;
            uint numStampHeaders = 0;
            if (amsCmd.AmsHeader.StateFlags != 4)
            {
                if (amsCmd.AmsHeader.StateFlags == 5)
                {
                    AdsErrorCode code;
                    switch (amsCmd.AmsHeader.CommandId)
                    {
                        case 1:
                        {
                            AdsVersion version = new AdsVersion();
                            if (amsCmd.AmsHeader.ErrCode != 0)
                            {
                                this.AdsReadDeviceInfoCon(amsCmd.AmsHeader.Sender, amsCmd.AmsHeader.HUser, (AdsErrorCode) amsCmd.AmsHeader.ErrCode, null, version);
                                return;
                            }
                            code = (AdsErrorCode) TcAdsParser.ReadUInt(data, 0);
                            version.Version = data[4];
                            version.Revision = data[5];
                            version.Build = TcAdsParser.ReadUShort(data, 6);
                            this.AdsReadDeviceInfoCon(amsCmd.AmsHeader.Sender, amsCmd.AmsHeader.HUser, code, TcAdsParser.ReadString(data, 8, 0x10), version);
                            return;
                        }
                        case 2:
                            if (amsCmd.AmsHeader.ErrCode != 0)
                            {
                                this.AdsReadCon(amsCmd.AmsHeader.Sender, amsCmd.AmsHeader.HUser, (AdsErrorCode) amsCmd.AmsHeader.ErrCode, 0, null);
                                return;
                            }
                            code = (AdsErrorCode) TcAdsParser.ReadUInt(data, 0);
                            cbLength = TcAdsParser.ReadUInt(data, 4);
                            this.AdsReadCon(amsCmd.AmsHeader.Sender, amsCmd.AmsHeader.HUser, code, cbLength, TcAdsParser.AdsData(data, 8, cbLength));
                            return;

                        case 3:
                            if (amsCmd.AmsHeader.ErrCode != 0)
                            {
                                this.AdsWriteCon(amsCmd.AmsHeader.Sender, amsCmd.AmsHeader.HUser, (AdsErrorCode) amsCmd.AmsHeader.ErrCode);
                                return;
                            }
                            code = (AdsErrorCode) TcAdsParser.ReadUInt(data, 0);
                            this.AdsWriteCon(amsCmd.AmsHeader.Sender, amsCmd.AmsHeader.HUser, code);
                            return;

                        case 4:
                            if (amsCmd.AmsHeader.ErrCode != 0)
                            {
                                this.AdsReadStateCon(amsCmd.AmsHeader.Sender, amsCmd.AmsHeader.HUser, (AdsErrorCode) amsCmd.AmsHeader.ErrCode, AdsState.Invalid, 0);
                                return;
                            }
                            code = (AdsErrorCode) TcAdsParser.ReadUInt(data, 0);
                            this.AdsReadStateCon(amsCmd.AmsHeader.Sender, amsCmd.AmsHeader.HUser, code, (AdsState) ((short) TcAdsParser.ReadUShort(data, 4)), TcAdsParser.ReadUShort(data, 6));
                            return;

                        case 5:
                            if (amsCmd.AmsHeader.ErrCode != 0)
                            {
                                this.AdsWriteControlCon(amsCmd.AmsHeader.Sender, amsCmd.AmsHeader.HUser, (AdsErrorCode) amsCmd.AmsHeader.ErrCode);
                                return;
                            }
                            code = (AdsErrorCode) TcAdsParser.ReadUInt(data, 0);
                            this.AdsWriteControlCon(amsCmd.AmsHeader.Sender, amsCmd.AmsHeader.HUser, code);
                            return;

                        case 6:
                            if (amsCmd.AmsHeader.ErrCode != 0)
                            {
                                this.AdsAddDeviceNotificationCon(amsCmd.AmsHeader.Sender, amsCmd.AmsHeader.HUser, (AdsErrorCode) amsCmd.AmsHeader.ErrCode, 0);
                                return;
                            }
                            code = (AdsErrorCode) TcAdsParser.ReadUInt(data, 0);
                            this.AdsAddDeviceNotificationCon(amsCmd.AmsHeader.Sender, amsCmd.AmsHeader.HUser, code, TcAdsParser.ReadUInt(data, 4));
                            return;

                        case 7:
                            if (amsCmd.AmsHeader.ErrCode != 0)
                            {
                                this.AdsDelDeviceNotificationCon(amsCmd.AmsHeader.Sender, amsCmd.AmsHeader.HUser, (AdsErrorCode) amsCmd.AmsHeader.ErrCode);
                                return;
                            }
                            code = (AdsErrorCode) TcAdsParser.ReadUInt(data, 0);
                            this.AdsDelDeviceNotificationCon(amsCmd.AmsHeader.Sender, amsCmd.AmsHeader.HUser, code);
                            return;

                        case 8:
                            break;

                        case 9:
                            if (amsCmd.AmsHeader.ErrCode != 0)
                            {
                                this.AdsReadWriteCon(amsCmd.AmsHeader.Sender, amsCmd.AmsHeader.HUser, (AdsErrorCode) amsCmd.AmsHeader.ErrCode, 0, null);
                                break;
                            }
                            code = (AdsErrorCode) TcAdsParser.ReadUInt(data, 0);
                            cbLength = TcAdsParser.ReadUInt(data, 4);
                            this.AdsReadWriteCon(amsCmd.AmsHeader.Sender, amsCmd.AmsHeader.HUser, code, cbLength, TcAdsParser.AdsData(data, 8, cbLength));
                            return;

                        default:
                            return;
                    }
                }
            }
            else
            {
                switch (amsCmd.AmsHeader.CommandId)
                {
                    case 1:
                        this.AdsReadDeviceInfoInd(amsCmd.AmsHeader.Sender, amsCmd.AmsHeader.HUser);
                        return;

                    case 2:
                        indexGroup = TcAdsParser.ReadUInt(data, 0);
                        this.AdsReadInd(amsCmd.AmsHeader.Sender, amsCmd.AmsHeader.HUser, indexGroup, TcAdsParser.ReadUInt(data, 4), TcAdsParser.DataLength(data, 8));
                        return;

                    case 3:
                        indexGroup = TcAdsParser.ReadUInt(data, 0);
                        cbLength = TcAdsParser.DataLength(data, 8);
                        this.AdsWriteInd(amsCmd.AmsHeader.Sender, amsCmd.AmsHeader.HUser, indexGroup, TcAdsParser.ReadUInt(data, 4), cbLength, TcAdsParser.AdsData(data, 12, cbLength));
                        return;

                    case 4:
                        this.AdsReadStateInd(amsCmd.AmsHeader.Sender, amsCmd.AmsHeader.HUser);
                        return;

                    case 5:
                    {
                        AdsState adsState = TcAdsParser.AdsState(data);
                        cbLength = TcAdsParser.DataLength(data, 4);
                        this.AdsWriteControlInd(amsCmd.AmsHeader.Sender, amsCmd.AmsHeader.HUser, adsState, TcAdsParser.ReadUShort(data, 2), cbLength, TcAdsParser.AdsData(data, 8, cbLength));
                        return;
                    }
                    case 6:
                        indexGroup = TcAdsParser.ReadUInt(data, 0);
                        this.AdsAddDeviceNotificationInd(amsCmd.AmsHeader.Sender, amsCmd.AmsHeader.HUser, indexGroup, TcAdsParser.ReadUInt(data, 4), TcAdsParser.DataLength(data, 8), (AdsTransMode) TcAdsParser.ReadUInt(data, 12), TcAdsParser.ReadUInt(data, 0x10), TcAdsParser.ReadUInt(data, 20));
                        return;

                    case 7:
                    {
                        uint hNotification = TcAdsParser.ReadUInt(data, 0);
                        this.AdsDelDeviceNotificationInd(amsCmd.AmsHeader.Sender, amsCmd.AmsHeader.HUser, hNotification);
                        return;
                    }
                    case 8:
                        if (!this._useSingleNotificationEventHandler)
                        {
                            goto TR_0008;
                        }
                        else
                        {
                            using (AdsBinaryReader reader = new AdsBinaryReader(new AdsStream(data)))
                            {
                                cbLength = reader.ReadUInt32();
                                numStampHeaders = reader.ReadUInt32();
                                this.AdsDeviceNotificationInd(amsCmd.AmsHeader.Sender, amsCmd.AmsHeader.HUser, numStampHeaders, reader);
                                break;
                            }
                            goto TR_0008;
                        }
                        break;

                    case 9:
                        indexGroup = TcAdsParser.ReadUInt(data, 0);
                        cbWriteLength = TcAdsParser.ReadUInt(data, 12);
                        this.AdsReadWriteInd(amsCmd.AmsHeader.Sender, amsCmd.AmsHeader.HUser, indexGroup, TcAdsParser.ReadUInt(data, 4), TcAdsParser.ReadUInt(data, 8), cbWriteLength, TcAdsParser.AdsData(data, 0x10, cbWriteLength));
                        return;

                    default:
                        return;
                }
            }
            return;
        TR_0008:
            cbLength = TcAdsParser.ReadUInt(data, 0);
            numStampHeaders = TcAdsParser.ReadUInt(data, 4);
            this.AdsDeviceNotificationInd(amsCmd.AmsHeader.Sender, amsCmd.AmsHeader.HUser, numStampHeaders, TcAdsParser.ReadNotificationStampHeaders(data, numStampHeaders));
        }

        internal void OnAmsServerException(TcAmsServerExEventArgs e)
        {
            if (this._adsServerExEvent != null)
            {
                this._adsServerExEvent(new TcAdsServerExEventArgs(e.Exception));
            }
        }

        [ComVisible(false)]
        public AmsAddress Address =>
            this._amsServer.Address;

        [ComVisible(false)]
        public bool IsConnected =>
            this._isConnected;

        internal int QueueSize =>
            this._amsServer.QueueSize;

        public bool IsDisposed =>
            this._disposed;

        public delegate void TcAdsServerExDelegate(TcAdsServerExEventArgs e);
    }
}

