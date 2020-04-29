namespace TwinCAT.Ads.SumCommand
{
    using System;
    using System.ComponentModel;
    using TwinCAT.TypeSystem;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    internal class HandleSumWriteAnyEntity : HandleSumEntity
    {
        public readonly System.Type Type;

        public HandleSumWriteAnyEntity(uint handle, System.Type tp, PrimitiveTypeConverter converter) : base(handle, 0, -1, converter)
        {
            this.Type = tp;
            if (tp == typeof(string))
            {
                base.writeLength = -1;
            }
            else
            {
                base.writeLength = PrimitiveTypeConverter.MarshalSize(tp);
            }
        }
    }
}

