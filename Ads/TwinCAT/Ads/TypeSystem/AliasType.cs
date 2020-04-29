namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using System.ComponentModel;
    using TwinCAT.Ads;
    using TwinCAT.Ads.Internal;
    using TwinCAT.TypeSystem;

    public sealed class AliasType : DataType, IAliasType, IDataType, IBitSize
    {
        private AdsDatatypeId _baseTypeId;
        private string _baseTypeName;
        private IDataType _baseType;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public AliasType(AdsDataTypeEntry entry) : base(DataTypeCategory.Alias, entry)
        {
            this._baseTypeId = entry.baseTypeId;
            this._baseTypeName = entry.typeName;
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public AliasType(string name, DataType baseType) : base(baseType)
        {
            base.category = DataTypeCategory.Alias;
            base.dataTypeId = AdsDatatypeId.ADST_BIGTYPE;
            base.name = name;
            this._baseTypeId = baseType.DataTypeId;
            this._baseType = baseType;
            this._baseTypeName = baseType.Name;
        }

        public override string ToString()
        {
            $"{base.Namespace} (Size: {base.Size}, Base: {this.BaseTypeName}, CAT: {base.Category})";
            return base.Name;
        }

        public string BaseTypeName =>
            this._baseTypeName;

        public IDataType BaseType
        {
            get
            {
                if (this._baseType == null)
                {
                    base.resolver.TryResolveType(this._baseTypeName, out this._baseType);
                }
                return this._baseType;
            }
        }

        public override bool IsPrimitive =>
            ((DataType) this.BaseType).IsPrimitive;

        public override bool IsContainer
        {
            get
            {
                DataType baseType = (DataType) this.BaseType;
                return ((baseType == null) ? base.IsContainer : baseType.IsContainer);
            }
        }

        public override Type ManagedType
        {
            get
            {
                DataType baseType = (DataType) this.BaseType;
                return baseType?.ManagedType;
            }
        }
    }
}

