namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using System.ComponentModel;
    using TwinCAT.Ads.Internal;
    using TwinCAT.TypeSystem;

    public class Field : Instance, IField, IAttributedInstance, IInstance, IBitSize
    {
        protected DataType parent;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public Field(DataType parent, AdsFieldEntry subEntry) : base(subEntry)
        {
            this.parent = parent;
        }

        public IDataType ParentType =>
            this.parent;
    }
}

