namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using System.ComponentModel;
    using TwinCAT.Ads;
    using TwinCAT.Ads.Internal;
    using TwinCAT.TypeSystem;

    public class StructType : DataType, IStructType, IDataType, IBitSize
    {
        private MemberCollection _members;
        private AdsDatatypeId _baseTypeId;
        private string _baseTypeName;
        private IDataType _baseType;
        private MemberCollection _allMembers;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public StructType(AdsDataTypeEntry entry) : base(DataTypeCategory.Struct, entry)
        {
            this._members = new MemberCollection();
            this._baseTypeId = entry.baseTypeId;
            this._baseTypeName = entry.typeName;
            for (int i = 0; i < entry.subItems; i++)
            {
                AdsFieldEntry subEntry = entry.subEntries[i];
                Member instance = new Member(this, subEntry);
                if (!this._members.isUnique(instance))
                {
                    ((IInstanceInternal) instance).SetInstanceName(this._members.createUniquepathName(instance));
                }
                this._members.Add(instance);
            }
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        protected override void OnBound(IBinder binder)
        {
            foreach (Member member in this._members)
            {
                member.Bind(binder);
            }
        }

        public bool IsDerived =>
            (this._baseTypeId != AdsDatatypeId.ADST_VOID);

        public ReadOnlyMemberCollection Members =>
            new ReadOnlyMemberCollection(this._members);

        public string BaseTypeName =>
            this._baseTypeName;

        public IDataType BaseType
        {
            get
            {
                if ((this.IsDerived && (this._baseType == null)) && !string.IsNullOrEmpty(this._baseTypeName))
                {
                    base.resolver.TryResolveType(this._baseTypeName, out this._baseType);
                }
                return this._baseType;
            }
        }

        public ReadOnlyMemberCollection AllMembers
        {
            get
            {
                if (!this.IsDerived)
                {
                    return new ReadOnlyMemberCollection(this._members);
                }
                if (this._allMembers == null)
                {
                    this._allMembers = new MemberCollection(this._members);
                    IDataType baseType = this.BaseType;
                    while (baseType != null)
                    {
                        IStructType type2 = (IStructType) baseType;
                        this._allMembers.AddRange(type2.Members);
                    }
                }
                return new ReadOnlyMemberCollection(this._allMembers);
            }
        }

        public virtual bool HasRpcMethods =>
            false;
    }
}

