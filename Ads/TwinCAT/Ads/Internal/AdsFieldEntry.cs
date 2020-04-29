namespace TwinCAT.Ads.Internal
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using TwinCAT.TypeSystem;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public class AdsFieldEntry : AdsDataTypeEntry, IAdsCustomMarshal<AdsFieldEntry>
    {
        public override bool TryGetPointerRef(out string referenceType) => 
            DataTypeStringParser.TryParsePointer(base.typeName, out referenceType);

        public override bool TryGetReference(out string referenceType) => 
            DataTypeStringParser.TryParseReference(base.typeName, out referenceType);

        public string SubItemName =>
            base.entryName;

        public int Offset =>
            ((int) base.offset);

        public bool IsStatic =>
            ((base.flags & (AdsDataTypeFlags.None | AdsDataTypeFlags.Static)) != AdsDataTypeFlags.None);

        public bool IsProperty =>
            ((base.flags & (AdsDataTypeFlags.None | AdsDataTypeFlags.PropItem)) != AdsDataTypeFlags.None);
    }
}

