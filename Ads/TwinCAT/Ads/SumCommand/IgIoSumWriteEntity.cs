namespace TwinCAT.Ads.SumCommand
{
    using System;
    using System.ComponentModel;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    internal class IgIoSumWriteEntity : IgIoSumEntity
    {
        public IgIoSumWriteEntity(uint indexGroup, uint indexOffset, int writeLength) : base(indexGroup, indexOffset, 0, writeLength)
        {
        }
    }
}

