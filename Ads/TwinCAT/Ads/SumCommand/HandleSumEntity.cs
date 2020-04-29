namespace TwinCAT.Ads.SumCommand
{
    using System;
    using System.ComponentModel;
    using TwinCAT.TypeSystem;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    internal class HandleSumEntity : SumDataEntity
    {
        public readonly uint Handle;
        internal readonly PrimitiveTypeConverter Converter;

        protected internal HandleSumEntity(uint handle, PrimitiveTypeConverter converter)
        {
            this.Handle = handle;
            this.Converter = converter;
        }

        protected internal HandleSumEntity(uint handle, int readLength, int writeLength, PrimitiveTypeConverter converter) : base(readLength, writeLength)
        {
            this.Handle = handle;
            this.Converter = converter;
        }
    }
}

