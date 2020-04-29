namespace TwinCAT.Ads.Server
{
    using System;

    internal class TcAmsCommand
    {
        private TcAmsHeader _amsHeader;
        private byte[] _data;

        internal TcAmsCommand(TcAmsHeader amsHeader, byte[] data)
        {
            this._amsHeader = amsHeader;
            this._data = data;
        }

        public TcAmsHeader AmsHeader
        {
            get => 
                this._amsHeader;
            set => 
                (this._amsHeader = value);
        }

        public byte[] Data
        {
            get => 
                this._data;
            set => 
                (this._data = value);
        }
    }
}

