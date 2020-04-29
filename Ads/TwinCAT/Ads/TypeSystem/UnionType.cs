namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using System.ComponentModel;
    using TwinCAT.Ads.Internal;
    using TwinCAT.TypeSystem;

    public sealed class UnionType : DataType, IUnionType, IDataType, IBitSize
    {
        private FieldCollection _fields;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        internal UnionType(AdsDataTypeEntry entry) : base(DataTypeCategory.Union, entry)
        {
            this._fields = new FieldCollection();
            for (int i = 0; i < entry.subItems; i++)
            {
                AdsFieldEntry subEntry = entry.subEntries[i];
                Member instance = new Member(this, subEntry);
                if (!this._fields.isUnique(instance))
                {
                    ((IInstanceInternal) instance).SetInstanceName(this._fields.createUniquepathName(instance));
                }
                this._fields.Add(instance);
            }
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        protected override void OnBound(IBinder binder)
        {
            foreach (Member member in this._fields)
            {
                member.Bind(binder);
            }
        }

        public ReadOnlyFieldCollection Fields =>
            new ReadOnlyFieldCollection(this._fields);
    }
}

