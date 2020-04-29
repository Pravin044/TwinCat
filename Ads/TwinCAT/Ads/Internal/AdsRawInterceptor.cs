namespace TwinCAT.Ads.Internal
{
    using System;
    using System.ComponentModel;
    using System.Resources;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using TwinCAT.Ads;

    internal class AdsRawInterceptor : ITcAdsRaw, ITcAdsRawPrimitives, ITcAdsRawAny, ITcAdsConnectionHandler, IAdsErrorInjector
    {
        private ITcAdsRaw _inner;
        private ICommunicationInterceptor _handler;
        private static ResourceManager _rm;

        internal AdsRawInterceptor(ITcAdsRaw inner, ICommunicationInterceptor handler)
        {
            this._inner = inner;
            this._handler = handler;
        }

        public AdsErrorCode AmsPortEnabled(bool throwAdsException, out bool enabled)
        {
            AdsErrorCode noError = AdsErrorCode.NoError;
            enabled = false;
            bool x = false;
            Func<AdsErrorCode> del = () => this._inner.AmsPortEnabled(false, out x);
            noError = this.Invoke(del);
            enabled = x;
            this.CheckResult(throwAdsException, noError);
            return noError;
        }

        private void CheckResult(bool throwAdsException, AdsErrorCode result)
        {
            if (throwAdsException && (result != AdsErrorCode.NoError))
            {
                ThrowAdsException(result);
            }
        }

        public AdsErrorCode InjectError(AdsErrorCode error, bool throwAdsException)
        {
            AdsErrorCode noError = AdsErrorCode.NoError;
            Func<AdsErrorCode> del = () => ((IAdsErrorInjector) this._inner).InjectError(error, false);
            noError = this.Invoke(del);
            this.CheckResult(throwAdsException, noError);
            return noError;
        }

        private AdsErrorCode Invoke(Func<AdsErrorCode> del) => 
            ((this._handler == null) ? del() : this._handler.Communicate(del));

        private AdsErrorCode Invoke(Action del, ref AdsErrorCode error)
        {
            if (this._handler != null)
            {
                this._handler.Communicate(del, ref error);
            }
            else
            {
                del();
            }
            return error;
        }

        private AdsErrorCode InvokeReadState(Func<AdsErrorCode> del, ref StateInfo stateInfo) => 
            ((this._handler == null) ? del() : this._handler.CommunicateReadState(del, ref stateInfo));

        private AdsErrorCode InvokeWriteState(Func<AdsErrorCode> del, ref StateInfo stateInfo) => 
            ((this._handler == null) ? del() : this._handler.CommunicateWriteState(del, ref stateInfo));

        public void OnBeforeDisconnected()
        {
            if (this._handler != null)
            {
                this._handler.BeforeDisconnect(() => AdsErrorCode.NoError);
            }
        }

        public void OnConnected()
        {
            if (this._handler != null)
            {
                this._handler.Connect(() => AdsErrorCode.NoError);
            }
        }

        public void OnDisconnected()
        {
            if (this._handler != null)
            {
                this._handler.Disconnect(() => AdsErrorCode.NoError);
            }
        }

        public AdsErrorCode Read(int variableHandle, int offset, int length, byte[] data, bool throwAdsException, out int dataRead)
        {
            AdsErrorCode result = AdsErrorCode.NoError;
            dataRead = 0;
            int x = 0;
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                result = code = this._inner.Read(variableHandle, offset, length, data, false, out x);
                return code;
            };
            result = this.Invoke(del);
            dataRead = x;
            this.CheckResult(throwAdsException, result);
            return result;
        }

        public unsafe AdsErrorCode Read(uint indexGroup, uint indexOffset, int length, void* data, bool throwAdsException, out int dataRead)
        {
            AdsErrorCode result = AdsErrorCode.NoError;
            dataRead = 0;
            int x = 0;
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                result = code = this._inner.Read(indexGroup, indexOffset, length, data, false, out x);
                return code;
            };
            result = this.Invoke(del);
            dataRead = x;
            this.CheckResult(throwAdsException, result);
            return result;
        }

        public AdsErrorCode Read(uint indexGroup, uint indexOffset, int offset, int length, byte[] data, bool throwAdsException, out int dataRead)
        {
            AdsErrorCode result = AdsErrorCode.NoError;
            dataRead = 0;
            int x = 0;
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                result = code = this._inner.Read(indexGroup, indexOffset, offset, length, data, false, out x);
                return code;
            };
            result = this.Invoke(del);
            dataRead = x;
            this.CheckResult(throwAdsException, result);
            return result;
        }

        public AdsErrorCode ReadAny(int variableHandle, Type type, bool throwAdsException, out object value)
        {
            AdsErrorCode result = AdsErrorCode.NoError;
            value = null;
            object x = 0;
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                result = code = this._inner.ReadAny(variableHandle, type, false, out x);
                return code;
            };
            result = this.Invoke(del);
            value = x;
            this.CheckResult(throwAdsException, result);
            return result;
        }

        public AdsErrorCode ReadAny(int variableHandle, Type type, int[] args, bool throwAdsException, out object value)
        {
            AdsErrorCode result = AdsErrorCode.NoError;
            value = null;
            object x = 0;
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                result = code = this._inner.ReadAny(variableHandle, type, args, false, out x);
                return code;
            };
            result = this.Invoke(del);
            value = x;
            this.CheckResult(throwAdsException, result);
            return result;
        }

        public AdsErrorCode ReadAny(uint indexGroup, uint indexOffset, Type type, bool throwAdsException, out object value)
        {
            AdsErrorCode result = AdsErrorCode.NoError;
            value = null;
            object x = 0;
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                result = code = this._inner.ReadAny(indexGroup, indexOffset, type, false, out x);
                return code;
            };
            result = this.Invoke(del);
            value = x;
            this.CheckResult(throwAdsException, result);
            return result;
        }

        public AdsErrorCode ReadAny(uint indexGroup, uint indexOffset, Type type, int[] args, bool throwAdsException, out object value)
        {
            AdsErrorCode result = AdsErrorCode.NoError;
            value = null;
            object x = 0;
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                result = code = this._inner.ReadAny(indexGroup, indexOffset, type, args, false, out x);
                return code;
            };
            result = this.Invoke(del);
            value = x;
            this.CheckResult(throwAdsException, result);
            return result;
        }

        public bool ReadBoolean(uint indexGroup, uint indexOffset, bool throwAdsException, out AdsErrorCode result)
        {
            bool value = false;
            AdsErrorCode x = AdsErrorCode.NoError;
            Action del = delegate {
                value = this._inner.ReadBoolean(indexGroup, indexOffset, false, out x);
            };
            result = this.Invoke(del, ref x);
            this.CheckResult(throwAdsException, result);
            return value;
        }

        public short ReadInt16(uint indexGroup, uint indexOffset, bool throwAdsException, out AdsErrorCode result)
        {
            short value = 0;
            AdsErrorCode x = AdsErrorCode.NoError;
            Action del = delegate {
                value = this._inner.ReadInt16(indexGroup, indexOffset, false, out x);
            };
            result = this.Invoke(del, ref x);
            this.CheckResult(throwAdsException, result);
            return value;
        }

        public int ReadInt32(uint indexGroup, uint indexOffset, bool throwAdsException, out AdsErrorCode result)
        {
            int value = 0;
            AdsErrorCode x = AdsErrorCode.NoError;
            Action del = delegate {
                value = this._inner.ReadInt32(indexGroup, indexOffset, false, out x);
            };
            result = this.Invoke(del, ref x);
            this.CheckResult(throwAdsException, result);
            return value;
        }

        public long ReadInt64(uint indexGroup, uint indexOffset, bool throwAdsException, out AdsErrorCode result)
        {
            long value = 0L;
            AdsErrorCode x = AdsErrorCode.NoError;
            Action del = delegate {
                value = this._inner.ReadInt64(indexGroup, indexOffset, false, out x);
            };
            result = this.Invoke(del, ref x);
            this.CheckResult(throwAdsException, result);
            return value;
        }

        public sbyte ReadInt8(uint indexGroup, uint indexOffset, bool throwAdsException, out AdsErrorCode result)
        {
            sbyte value = 0;
            AdsErrorCode x = AdsErrorCode.NoError;
            Action del = delegate {
                value = this._inner.ReadInt8(indexGroup, indexOffset, false, out x);
            };
            result = this.Invoke(del, ref x);
            this.CheckResult(throwAdsException, result);
            return value;
        }

        public float ReadReal32(uint indexGroup, uint indexOffset, bool throwAdsException, out AdsErrorCode result)
        {
            float value = 0f;
            AdsErrorCode x = AdsErrorCode.NoError;
            Action del = delegate {
                value = this._inner.ReadReal32(indexGroup, indexOffset, false, out x);
            };
            result = this.Invoke(del, ref x);
            this.CheckResult(throwAdsException, result);
            return value;
        }

        public double ReadReal64(uint indexGroup, uint indexOffset, bool throwAdsException, out AdsErrorCode result)
        {
            double value = 0.0;
            AdsErrorCode x = AdsErrorCode.NoError;
            Action del = delegate {
                value = this._inner.ReadReal64(indexGroup, indexOffset, false, out x);
            };
            result = this.Invoke(del, ref x);
            this.CheckResult(throwAdsException, result);
            return value;
        }

        public AdsErrorCode ReadState(bool throwAdsException, out StateInfo stateInfo)
        {
            AdsErrorCode result = AdsErrorCode.NoError;
            StateInfo x = new StateInfo();
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                result = code = this._inner.ReadState(false, out x);
                return code;
            };
            result = this.InvokeReadState(del, ref x);
            stateInfo = x;
            this.CheckResult(throwAdsException, result);
            return result;
        }

        public string ReadString(int variableHandle, int characters, Encoding encoding, bool throwAdsException, out AdsErrorCode result)
        {
            string value = null;
            AdsErrorCode x = AdsErrorCode.NoError;
            Action del = delegate {
                value = this._inner.ReadString(variableHandle, characters, encoding, false, out x);
            };
            result = this.Invoke(del, ref x);
            this.CheckResult(throwAdsException, result);
            return value;
        }

        public string ReadString(uint indexGroup, uint indexOffset, int characters, Encoding encoding, bool throwAdsException, out AdsErrorCode result)
        {
            string value = null;
            AdsErrorCode x = AdsErrorCode.NoError;
            Action del = delegate {
                value = this._inner.ReadString(indexGroup, indexOffset, characters, encoding, false, out x);
            };
            result = this.Invoke(del, ref x);
            this.CheckResult(throwAdsException, result);
            return value;
        }

        public object ReadStruct(uint indexGroup, uint indexOffset, Type structureType, bool throwAdsException, out AdsErrorCode result)
        {
            object value = 0;
            AdsErrorCode x = AdsErrorCode.NoError;
            Action del = delegate {
                value = this._inner.ReadStruct(indexGroup, indexOffset, structureType, false, out x);
            };
            result = this.Invoke(del, ref x);
            this.CheckResult(throwAdsException, result);
            return value;
        }

        public ushort ReadUInt16(uint indexGroup, uint indexOffset, bool throwAdsException, out AdsErrorCode result)
        {
            ushort value = 0;
            AdsErrorCode x = AdsErrorCode.NoError;
            Action del = delegate {
                value = this._inner.ReadUInt16(indexGroup, indexOffset, false, out x);
            };
            result = this.Invoke(del, ref x);
            this.CheckResult(throwAdsException, result);
            return value;
        }

        public uint ReadUInt32(uint indexGroup, uint indexOffset, bool throwAdsException, out AdsErrorCode result)
        {
            uint value = 0;
            AdsErrorCode x = AdsErrorCode.NoError;
            Action del = delegate {
                value = this._inner.ReadUInt32(indexGroup, indexOffset, false, out x);
            };
            result = this.Invoke(del, ref x);
            this.CheckResult(throwAdsException, result);
            return value;
        }

        public ulong ReadUInt64(uint indexGroup, uint indexOffset, bool throwAdsException, out AdsErrorCode result)
        {
            ulong value = 0UL;
            AdsErrorCode x = AdsErrorCode.NoError;
            Action del = delegate {
                value = this._inner.ReadUInt64(indexGroup, indexOffset, false, out x);
            };
            result = this.Invoke(del, ref x);
            this.CheckResult(throwAdsException, result);
            return value;
        }

        public byte ReadUInt8(uint indexGroup, uint indexOffset, bool throwAdsException, out AdsErrorCode result)
        {
            byte value = 0;
            AdsErrorCode x = AdsErrorCode.NoError;
            Action del = delegate {
                value = this._inner.ReadUInt8(indexGroup, indexOffset, false, out x);
            };
            result = this.Invoke(del, ref x);
            this.CheckResult(throwAdsException, result);
            return value;
        }

        public AdsErrorCode ReadWrite(uint indexGroup, uint indexOffset, string wrValue, bool throwAdsException, out uint value)
        {
            AdsErrorCode result = AdsErrorCode.NoError;
            uint x = 0;
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                result = code = this._inner.ReadWrite(indexGroup, indexOffset, wrValue, false, out x);
                return code;
            };
            result = this.Invoke(del);
            value = x;
            this.CheckResult(throwAdsException, result);
            return result;
        }

        public unsafe AdsErrorCode ReadWrite(uint indexGroup, uint indexOffset, int readLength, void* readData, int writeLength, void* writeData, bool throwAdsException, out int dataRead)
        {
            AdsErrorCode result = AdsErrorCode.NoError;
            int x = 0;
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                result = code = this._inner.ReadWrite(indexGroup, indexOffset, readLength, readData, writeLength, writeData, false, out x);
                return code;
            };
            result = this.Invoke(del);
            dataRead = x;
            this.CheckResult(throwAdsException, result);
            return result;
        }

        public AdsErrorCode ReadWrite(int variableHandle, int rdOffset, int rdLength, byte[] rdData, int wrOffset, int wrLength, byte[] wrData, bool throwAdsException, out int dataRead)
        {
            AdsErrorCode result = AdsErrorCode.NoError;
            int x = 0;
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                result = code = this._inner.ReadWrite(variableHandle, rdOffset, rdLength, rdData, wrOffset, wrLength, wrData, false, out x);
                return code;
            };
            result = this.Invoke(del);
            dataRead = x;
            this.CheckResult(throwAdsException, result);
            return result;
        }

        public AdsErrorCode ReadWrite(uint indexGroup, uint indexOffset, int rdOffset, int rdLength, byte[] rdData, int wrOffset, int wrLength, byte[] wrData, bool throwAdsException, out int dataRead)
        {
            AdsErrorCode result = AdsErrorCode.NoError;
            int x = 0;
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                result = code = this._inner.ReadWrite(indexGroup, indexOffset, rdOffset, rdLength, rdData, wrOffset, wrLength, wrData, false, out x);
                return code;
            };
            result = this.Invoke(del);
            dataRead = x;
            this.CheckResult(throwAdsException, result);
            return result;
        }

        private static void ThrowAdsException(AdsErrorCode adsErrorCode)
        {
            if (adsErrorCode != AdsErrorCode.NoError)
            {
                if (_rm == null)
                {
                    _rm = new ResourceManager("TwinCAT.Ads.Resource", typeof(TcAdsDllWrapper).Assembly);
                }
                string str = _rm.GetString("AdsError_" + ((uint) adsErrorCode).ToString());
                if (str == null)
                {
                    str = "A unknown Ads-Error has occurred.";
                }
                throw new AdsErrorException($"Ads-Error 0x{(uint) adsErrorCode:X} : {str}", adsErrorCode);
            }
        }

        public AdsErrorCode TryCreateVariableHandle(string variableName, bool throwAdsException, out int handle)
        {
            AdsErrorCode error = AdsErrorCode.NoError;
            handle = 0;
            int x = 0;
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                error = code = this._inner.TryCreateVariableHandle(variableName, false, out x);
                return code;
            };
            error = this.Invoke(del);
            handle = x;
            this.CheckResult(throwAdsException, error);
            return error;
        }

        public AdsErrorCode TryDeleteVariableHandle(int variableHandle, bool throwAdsException)
        {
            AdsErrorCode error = AdsErrorCode.NoError;
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                error = code = this._inner.TryDeleteVariableHandle(variableHandle, true);
                return code;
            };
            error = this.Invoke(del);
            this.CheckResult(throwAdsException, error);
            return error;
        }

        public AdsErrorCode Write(uint indexGroup, uint indexOffset, bool val, bool throwAdsException)
        {
            AdsErrorCode result = AdsErrorCode.NoError;
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                result = code = this._inner.Write(indexGroup, indexOffset, val, false);
                return code;
            };
            result = this.Invoke(del);
            this.CheckResult(throwAdsException, result);
            return result;
        }

        public AdsErrorCode Write(uint indexGroup, uint indexOffset, byte val, bool throwAdsException)
        {
            AdsErrorCode result = AdsErrorCode.NoError;
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                result = code = this._inner.Write(indexGroup, indexOffset, val, false);
                return code;
            };
            result = this.Invoke(del);
            this.CheckResult(throwAdsException, result);
            return result;
        }

        public AdsErrorCode Write(uint indexGroup, uint indexOffset, double val, bool throwAdsException)
        {
            AdsErrorCode result = AdsErrorCode.NoError;
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                result = code = this._inner.Write(indexGroup, indexOffset, val, false);
                return code;
            };
            result = this.Invoke(del);
            this.CheckResult(throwAdsException, result);
            return result;
        }

        public AdsErrorCode Write(uint indexGroup, uint indexOffset, short val, bool throwAdsException)
        {
            AdsErrorCode result = AdsErrorCode.NoError;
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                result = code = this._inner.Write(indexGroup, indexOffset, val, false);
                return code;
            };
            result = this.Invoke(del);
            this.CheckResult(throwAdsException, result);
            return result;
        }

        public AdsErrorCode Write(uint indexGroup, uint indexOffset, int val, bool throwAdsException)
        {
            AdsErrorCode result = AdsErrorCode.NoError;
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                result = code = this._inner.Write(indexGroup, indexOffset, val, false);
                return code;
            };
            result = this.Invoke(del);
            this.CheckResult(throwAdsException, result);
            return result;
        }

        public AdsErrorCode Write(uint indexGroup, uint indexOffset, long val, bool throwAdsException)
        {
            AdsErrorCode result = AdsErrorCode.NoError;
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                result = code = this._inner.Write(indexGroup, indexOffset, val, false);
                return code;
            };
            result = this.Invoke(del);
            this.CheckResult(throwAdsException, result);
            return result;
        }

        public AdsErrorCode Write(uint indexGroup, uint indexOffset, object val, bool throwAdsException)
        {
            AdsErrorCode result = AdsErrorCode.NoError;
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                result = code = this._inner.Write(indexGroup, indexOffset, val, false);
                return code;
            };
            result = this.Invoke(del);
            this.CheckResult(throwAdsException, result);
            return result;
        }

        public AdsErrorCode Write(uint indexGroup, uint indexOffset, sbyte val, bool throwAdsException)
        {
            AdsErrorCode result = AdsErrorCode.NoError;
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                result = code = this._inner.Write(indexGroup, indexOffset, val, false);
                return code;
            };
            result = this.Invoke(del);
            this.CheckResult(throwAdsException, result);
            return result;
        }

        public AdsErrorCode Write(uint indexGroup, uint indexOffset, float val, bool throwAdsException)
        {
            AdsErrorCode result = AdsErrorCode.NoError;
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                result = code = this._inner.Write(indexGroup, indexOffset, val, false);
                return code;
            };
            result = this.Invoke(del);
            this.CheckResult(throwAdsException, result);
            return result;
        }

        public AdsErrorCode Write(uint indexGroup, uint indexOffset, ushort val, bool throwAdsException)
        {
            AdsErrorCode result = AdsErrorCode.NoError;
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                result = code = this._inner.Write(indexGroup, indexOffset, val, false);
                return code;
            };
            result = this.Invoke(del);
            this.CheckResult(throwAdsException, result);
            return result;
        }

        public AdsErrorCode Write(uint indexGroup, uint indexOffset, uint val, bool throwAdsException)
        {
            AdsErrorCode result = AdsErrorCode.NoError;
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                result = code = this._inner.Write(indexGroup, indexOffset, val, false);
                return code;
            };
            result = this.Invoke(del);
            this.CheckResult(throwAdsException, result);
            return result;
        }

        public AdsErrorCode Write(int variableHandle, int offset, int length, byte[] data, bool throwAdsException)
        {
            AdsErrorCode result = AdsErrorCode.NoError;
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                result = code = this._inner.Write(variableHandle, offset, length, data, false);
                return code;
            };
            result = this.Invoke(del);
            this.CheckResult(throwAdsException, result);
            return result;
        }

        public unsafe AdsErrorCode Write(uint indexGroup, uint indexOffset, int length, void* data, bool throwAdsException)
        {
            AdsErrorCode result = AdsErrorCode.NoError;
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                result = code = this._inner.Write(indexGroup, indexOffset, length, data, false);
                return code;
            };
            result = this.Invoke(del);
            this.CheckResult(throwAdsException, result);
            return result;
        }

        public AdsErrorCode Write(uint indexGroup, uint indexOffset, string val, int characters, bool throwAdsException)
        {
            AdsErrorCode result = AdsErrorCode.NoError;
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                result = code = this._inner.Write(indexGroup, indexOffset, val, characters, false);
                return code;
            };
            result = this.Invoke(del);
            this.CheckResult(throwAdsException, result);
            return result;
        }

        public AdsErrorCode Write(uint indexGroup, uint indexOffset, int offset, int length, byte[] data, bool throwAdsException)
        {
            AdsErrorCode result = AdsErrorCode.NoError;
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                result = code = this._inner.Write(indexGroup, indexOffset, offset, length, data, false);
                return code;
            };
            result = this.Invoke(del);
            this.CheckResult(throwAdsException, result);
            return result;
        }

        public AdsErrorCode WriteAny(int variableHandle, object value, bool throwAdsException)
        {
            AdsErrorCode result = AdsErrorCode.NoError;
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                result = code = this._inner.WriteAny(variableHandle, value, false);
                return code;
            };
            result = this.Invoke(del);
            this.CheckResult(throwAdsException, result);
            return result;
        }

        public AdsErrorCode WriteAny(int variableHandle, object value, int[] args, bool throwAdsException)
        {
            AdsErrorCode result = AdsErrorCode.NoError;
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                result = code = this._inner.WriteAny(variableHandle, value, args, false);
                return code;
            };
            result = this.Invoke(del);
            this.CheckResult(throwAdsException, result);
            return result;
        }

        public AdsErrorCode WriteAny(uint indexGroup, uint indexOffset, object value, bool throwAdsException)
        {
            AdsErrorCode result = AdsErrorCode.NoError;
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                result = code = this._inner.WriteAny(indexGroup, indexOffset, value, false);
                return code;
            };
            result = this.Invoke(del);
            this.CheckResult(throwAdsException, result);
            return result;
        }

        public AdsErrorCode WriteAny(uint indexGroup, uint indexOffset, object value, int[] args, bool throwAdsException)
        {
            AdsErrorCode result = AdsErrorCode.NoError;
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                result = code = this._inner.WriteAny(indexGroup, indexOffset, value, args, false);
                return code;
            };
            result = this.Invoke(del);
            this.CheckResult(throwAdsException, result);
            return result;
        }

        public AdsErrorCode WriteControl(StateInfo stateInfo, byte[] data, int offset, int length, bool throwAdsException)
        {
            AdsErrorCode result = AdsErrorCode.NoError;
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                result = code = this._inner.WriteControl(stateInfo, data, offset, length, false);
                return code;
            };
            result = this.InvokeWriteState(del, ref stateInfo);
            this.CheckResult(throwAdsException, result);
            return result;
        }

        [Obsolete("This method is potentially unsafe!"), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public AdsErrorCode WriteString(int variableHandle, string str, int characters, Encoding encoding, bool throwAdsException)
        {
            AdsErrorCode result = AdsErrorCode.NoError;
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                result = code = this._inner.WriteString(variableHandle, str, characters, encoding, false);
                return code;
            };
            result = this.Invoke(del);
            this.CheckResult(throwAdsException, result);
            return result;
        }

        [Obsolete("This method is potentially unsafe!"), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public AdsErrorCode WriteString(uint indexGroup, uint indexOffset, string str, int characters, Encoding encoding, bool throwAdsException)
        {
            AdsErrorCode result = AdsErrorCode.NoError;
            Func<AdsErrorCode> del = delegate {
                AdsErrorCode code;
                result = code = this._inner.WriteString(indexGroup, indexOffset, str, characters, encoding, false);
                return code;
            };
            result = this.Invoke(del);
            this.CheckResult(throwAdsException, result);
            return result;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly AdsRawInterceptor.<>c <>9 = new AdsRawInterceptor.<>c();
            public static Func<AdsErrorCode> <>9__61_0;
            public static Func<AdsErrorCode> <>9__62_0;
            public static Func<AdsErrorCode> <>9__63_0;

            internal AdsErrorCode <OnBeforeDisconnected>b__62_0() => 
                AdsErrorCode.NoError;

            internal AdsErrorCode <OnConnected>b__61_0() => 
                AdsErrorCode.NoError;

            internal AdsErrorCode <OnDisconnected>b__63_0() => 
                AdsErrorCode.NoError;
        }
    }
}

