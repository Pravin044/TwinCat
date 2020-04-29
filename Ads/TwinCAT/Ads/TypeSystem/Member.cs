namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using TwinCAT.Ads.Internal;
    using TwinCAT.TypeSystem;

    [DebuggerDisplay("Name = { instanceName }, Type = {typeName}, Size = {size}, Offset = {offset}, Category = {category}, Static = {staticAddress}")]
    public sealed class Member : Field, IMember, IField, IAttributedInstance, IInstance, IBitSize
    {
        private int offset;
        private AdsDataTypeFlags _memberFlags;
        private uint _typeHashValue;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public Member(DataType parent, AdsFieldEntry subEntry) : base(parent, subEntry)
        {
            this.offset = (int) subEntry.offset;
            this._memberFlags = subEntry.Flags;
            this._typeHashValue = subEntry.typeHashValue;
        }

        public int Offset =>
            this.offset;

        public int BitOffset =>
            (!base.IsBitType ? (this.offset * 8) : this.offset);

        public int ByteOffset
        {
            get
            {
                if (base.IsBitType)
                {
                    return (this.offset / 8);
                }
                return this.offset;
            }
        }

        internal AdsDataTypeFlags MemberFlags =>
            this._memberFlags;

        internal uint TypeHashValue =>
            this._typeHashValue;
    }
}

