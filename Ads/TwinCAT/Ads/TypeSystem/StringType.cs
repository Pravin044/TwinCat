namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using System.ComponentModel;
    using System.Text;
    using TwinCAT.Ads.Internal;
    using TwinCAT.TypeSystem;

    public sealed class StringType : DataType, IStringType, IDataType, IBitSize
    {
        private int length;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public StringType(int length) : base($"STRING({length})", AdsDatatypeId.ADST_STRING, DataTypeCategory.String, 0, typeof(string))
        {
            int byteCount = System.Text.Encoding.Default.GetByteCount("a");
            base.size = (length + 1) * byteCount;
            base.flags = AdsDataTypeFlags.DataType;
            this.length = length;
        }

        internal StringType(AdsDataTypeEntry entry) : base(DataTypeCategory.String, entry)
        {
            base.dotnetType = typeof(string);
        }

        public override string ToString()
        {
            $"{base.Namespace} (Size: {base.Size}, Length: {this.Length}, CAT: {base.Category})";
            return base.Name;
        }

        public int Length =>
            this.length;

        public bool IsFixedLength =>
            (this.length >= 0);

        public System.Text.Encoding Encoding =>
            System.Text.Encoding.Default;
    }
}

