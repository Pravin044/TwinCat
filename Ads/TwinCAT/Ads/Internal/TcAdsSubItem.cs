namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Diagnostics;
    using TwinCAT.Ads;
    using TwinCAT.TypeSystem;

    [DebuggerDisplay("Entry = {SubItemName} Offset = {Offset} Name = {Name} : {BaseTypeName}")]
    internal class TcAdsSubItem : TcAdsDataType, ITcAdsSubItem, ITcAdsDataType, IDataType, IBitSize, IResolvableType
    {
        private string _subItemName;

        public TcAdsSubItem(AdsFieldEntry adsFieldEntry, IDataTypeResolver typeResolver) : base(adsFieldEntry, typeResolver)
        {
            this._subItemName = string.Empty;
            base._typeName = adsFieldEntry.Name;
            this._subItemName = adsFieldEntry.SubItemName;
        }

        internal void AlignSubItemToType(TcAdsDataType subEntryType)
        {
            if (base._dataTypeId != subEntryType.DataTypeId)
            {
                base._dataTypeId = subEntryType.DataTypeId;
            }
            if ((base._arrayInfo == null) && (subEntryType.ArrayInfo != null))
            {
                base._arrayInfo = subEntryType.ArrayInfo;
            }
        }

        public override bool IsSubItem =>
            true;

        public string SubItemName =>
            this._subItemName;

        public int Offset =>
            ((int) base._offset);

        public bool IsPersistent =>
            ((base.Flags & (AdsDataTypeFlags.None | AdsDataTypeFlags.Persistent)) == (AdsDataTypeFlags.None | AdsDataTypeFlags.Persistent));

        public bool IsStatic =>
            ((base.Flags & (AdsDataTypeFlags.None | AdsDataTypeFlags.Static)) == (AdsDataTypeFlags.None | AdsDataTypeFlags.Static));

        public bool IsProperty =>
            ((base.Flags & (AdsDataTypeFlags.None | AdsDataTypeFlags.PropItem)) == (AdsDataTypeFlags.None | AdsDataTypeFlags.PropItem));
    }
}

