namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using System.ComponentModel;
    using TwinCAT.Ads;
    using TwinCAT.Ads.Internal;
    using TwinCAT.TypeSystem;

    public sealed class PrimitiveType : DataType, IPrimitiveType, IDataType, IBitSize
    {
        private PrimitiveTypeFlags _primitiveFlags;

        internal PrimitiveType(AdsDataTypeEntry entry, PrimitiveTypeFlags flags, Type dotnetType) : base(DataTypeCategory.Primitive, entry)
        {
            this._primitiveFlags = flags;
            base.dotnetType = dotnetType;
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public PrimitiveType(string name, AdsDatatypeId dataTypeId, int byteSize, PrimitiveTypeFlags flags, Type dotnetType) : base(name, dataTypeId, DataTypeCategory.Primitive, byteSize, dotnetType)
        {
            this._primitiveFlags = flags;
        }

        public PrimitiveTypeFlags PrimitiveFlags =>
            this._primitiveFlags;
    }
}

