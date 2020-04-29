namespace TwinCAT.Ads.SumCommand
{
    using System;
    using System.ComponentModel;
    using TwinCAT.TypeSystem;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    internal class HandleSumReadAnyEntity : HandleSumEntity
    {
        public readonly AnyTypeSpecifier TypeSpec;

        public HandleSumReadAnyEntity(uint handle, int strLen, PrimitiveTypeConverter converter) : base(handle, -1, 0, converter)
        {
            this.TypeSpec = new AnyTypeSpecifier(typeof(string), strLen);
            base.readLength = converter.MarshalSize(strLen);
        }

        public HandleSumReadAnyEntity(uint handle, Type tp, PrimitiveTypeConverter converter) : base(handle, converter)
        {
            base.readLength = PrimitiveTypeConverter.MarshalSize(tp);
            base.writeLength = 0;
            this.TypeSpec = new AnyTypeSpecifier(tp);
        }

        public HandleSumReadAnyEntity(uint handle, Type arrayType, AnyTypeSpecifier anyType, PrimitiveTypeConverter converter) : base(handle, -1, 0, converter)
        {
            this.TypeSpec = anyType;
            int size = 0;
            converter.TryGetArrayMarshalSize(this.TypeSpec, out size);
            base.readLength = size;
        }
    }
}

