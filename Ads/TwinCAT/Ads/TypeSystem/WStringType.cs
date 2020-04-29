namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using System.ComponentModel;
    using System.Text;
    using TwinCAT.Ads.Internal;
    using TwinCAT.TypeSystem;

    public sealed class WStringType : DataType, IStringType, IDataType, IBitSize
    {
        private int length;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public WStringType(int length) : base($"WSTRING({length})", AdsDatatypeId.ADST_WSTRING, DataTypeCategory.String, 0, typeof(string))
        {
            int byteCount = System.Text.Encoding.Unicode.GetByteCount("a");
            base.size = (length + 1) * byteCount;
            base.flags = AdsDataTypeFlags.DataType;
            this.length = length;
        }

        public override string ToString()
        {
            $"{base.Namespace} (Size: {base.Size}, Length: {this.Length}, CAT: {base.Category})";
            return base.Name;
        }

        public int Length =>
            this.length;

        public System.Text.Encoding Encoding =>
            System.Text.Encoding.Unicode;

        public bool IsFixedLength =>
            (this.length >= 0);
    }
}

