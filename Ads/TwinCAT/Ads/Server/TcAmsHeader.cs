namespace TwinCAT.Ads.Server
{
    using System;
    using System.IO;
    using TwinCAT.Ads;

    internal class TcAmsHeader
    {
        private AmsAddress _target;
        private AmsAddress _sender;
        private ushort _commandId;
        private ushort _stateFlags;
        private uint _cbData;
        private uint _errCode;
        private uint _hUser;

        internal TcAmsHeader(TcMarshallableAmsHeader mHeader)
        {
            this._target = new AmsAddress(mHeader.targetNetId, mHeader.targetPort);
            this._sender = new AmsAddress(mHeader.senderNetId, mHeader.senderPort);
            this._commandId = mHeader.cmdId;
            this._stateFlags = mHeader.stateFlags;
            this._cbData = mHeader.cbData;
            this._errCode = mHeader.errCode;
            this._hUser = mHeader.hUser;
        }

        internal TcAmsHeader(byte[] data)
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(data));
            byte[] buffer = new byte[6];
            reader.Read(buffer, 0, 6);
            this._target = new AmsAddress(buffer, reader.ReadUInt16());
            reader.Read(buffer, 0, 6);
            this._sender = new AmsAddress(buffer, reader.ReadUInt16());
            this._commandId = reader.ReadUInt16();
            this._stateFlags = reader.ReadUInt16();
            this._cbData = reader.ReadUInt32();
            this._errCode = reader.ReadUInt32();
            this._hUser = reader.ReadUInt32();
        }

        internal TcAmsHeader(BinaryReader reader)
        {
            byte[] buffer = new byte[6];
            reader.Read(buffer, 0, 6);
            this._target = new AmsAddress(buffer, reader.ReadUInt16());
            reader.Read(buffer, 0, 6);
            this._sender = new AmsAddress(buffer, reader.ReadUInt16());
            this._commandId = reader.ReadUInt16();
            this._stateFlags = reader.ReadUInt16();
            this._cbData = reader.ReadUInt32();
            this._errCode = reader.ReadUInt32();
            this._hUser = reader.ReadUInt32();
        }

        internal TcAmsHeader(AmsAddress target, AmsAddress sender, ushort commandId, ushort stateFlags, uint cbData, uint errCode, uint hUser)
        {
            this._target = target;
            this._sender = sender;
            this._commandId = commandId;
            this._stateFlags = stateFlags;
            this._cbData = cbData;
            this._errCode = errCode;
            this._hUser = hUser;
        }

        internal TcMarshallableAmsHeader GetMarshallableHeader()
        {
            TcMarshallableAmsHeader header = new TcMarshallableAmsHeader();
            this._target.NetId.ToBytes().CopyTo(header.targetNetId, 0);
            header.targetPort = (ushort) this._target.Port;
            this._sender.NetId.ToBytes().CopyTo(header.senderNetId, 0);
            header.senderPort = (ushort) this._sender.Port;
            header.cmdId = this._commandId;
            header.stateFlags = this._stateFlags;
            header.cbData = this._cbData;
            header.errCode = this._errCode;
            header.hUser = this._hUser;
            return header;
        }

        internal void SwitchTarget()
        {
            AmsAddress address = this._sender;
            this._sender = this._target;
            this._target = address;
        }

        internal AmsAddress Target
        {
            get => 
                this._target;
            set => 
                (this._target = value);
        }

        internal AmsAddress Sender
        {
            get => 
                this._sender;
            set => 
                (this._sender = value);
        }

        internal ushort CommandId
        {
            get => 
                this._commandId;
            set => 
                (this._commandId = value);
        }

        internal ushort StateFlags
        {
            get => 
                this._stateFlags;
            set => 
                (this._stateFlags = value);
        }

        internal uint CbData
        {
            get => 
                this._cbData;
            set => 
                (this._cbData = value);
        }

        internal uint ErrCode
        {
            get => 
                this._errCode;
            set => 
                (this._errCode = value);
        }

        internal uint HUser
        {
            get => 
                this._hUser;
            set => 
                (this._hUser = value);
        }
    }
}

