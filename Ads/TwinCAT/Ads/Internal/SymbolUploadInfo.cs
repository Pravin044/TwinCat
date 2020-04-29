namespace TwinCAT.Ads.Internal
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Text;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public class SymbolUploadInfo
    {
        private int _version;
        private uint _symbolCount;
        private uint _symbolsBlockSize;
        private uint _dataTypeCount;
        private uint _dataTypesBlockSize;
        private uint _maxDynamicSymbolCount;
        private uint _usedDynamicSymbolCount;
        private uint _invalidDynamicSymbolCount;
        private int _encodingCodePage;
        private SymbolUploadFlags _flags;
        private uint[] _reserved;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public SymbolUploadInfo()
        {
            this._version = 1;
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public SymbolUploadInfo(BinaryReader reader, int version)
        {
            this._version = version;
            this._symbolCount = reader.ReadUInt32();
            this._symbolsBlockSize = reader.ReadUInt32();
            if (version >= 2)
            {
                this._dataTypeCount = reader.ReadUInt32();
                this._dataTypesBlockSize = reader.ReadUInt32();
                this._maxDynamicSymbolCount = reader.ReadUInt32();
                this._usedDynamicSymbolCount = reader.ReadUInt32();
                if (version >= 3)
                {
                    this._invalidDynamicSymbolCount = reader.ReadUInt32();
                    this._encodingCodePage = (int) reader.ReadUInt32();
                    this._flags = (SymbolUploadFlags) reader.ReadUInt32();
                    this._reserved = new uint[7];
                    for (int i = 0; i < 7; i++)
                    {
                        this._reserved[i] = reader.ReadUInt32();
                    }
                }
            }
        }

        internal static int CalcVersion(int readBytes)
        {
            int num = 1;
            if (readBytes == 8)
            {
                num = 1;
            }
            else if (readBytes == 0x18)
            {
                num = 2;
            }
            else if (readBytes == 0x40)
            {
                num = 3;
            }
            return num;
        }

        internal string Dump() => 
            $"TypeCount: {this._dataTypeCount}, TypeSize: {this._dataTypesBlockSize} bytes, SymbolCount: {this._symbolCount}, SymbolSize: {this._symbolsBlockSize} bytes, DynSymbols: {this._usedDynamicSymbolCount}, MaxDynSymbols: {this._maxDynamicSymbolCount}";

        public override string ToString() => 
            $"{base.GetType().Name} ({this.Dump()})";

        public int Version =>
            this._version;

        public int SymbolCount =>
            ((int) this._symbolCount);

        public int SymbolsBlockSize =>
            ((int) this._symbolsBlockSize);

        public int DataTypeCount =>
            ((int) this._dataTypeCount);

        public int DataTypesBlockSize =>
            ((int) this._dataTypesBlockSize);

        public int MaxDynamicSymbolCount =>
            ((int) this._maxDynamicSymbolCount);

        public int UsedDynamicSymbolCount =>
            ((int) this._usedDynamicSymbolCount);

        public int InvalidDynamicSymbolCount =>
            ((int) this._invalidDynamicSymbolCount);

        public Encoding StringEncoding =>
            ((this._encodingCodePage == 0) ? Encoding.Default : Encoding.GetEncoding(this._encodingCodePage));

        public SymbolUploadFlags Flags =>
            this._flags;

        public int TargetPointerSize
        {
            get
            {
                if ((this._version != 3) || (this._encodingCodePage == 0))
                {
                    return 0;
                }
                return (((this._flags & SymbolUploadFlags.Is64BitPlatform) != SymbolUploadFlags.Is64BitPlatform) ? 4 : 8);
            }
        }

        public bool ContainsBaseTypes =>
            ((this._flags & SymbolUploadFlags.IncludesBaseTypes) == SymbolUploadFlags.IncludesBaseTypes);
    }
}

