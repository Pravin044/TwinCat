namespace TwinCAT.Ads.TypeSystem
{
    using System;

    public sealed class BitMappingType : DataType
    {
        public BitMappingType(string name, int bitSize, Type dotnetType) : base(name, AdsDatatypeId.ADST_BIT, DataTypeCategory.Primitive, bitSize, dotnetType, AdsDataTypeFlags.BitValues | AdsDataTypeFlags.DataType)
        {
        }
    }
}

