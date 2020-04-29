namespace TwinCAT.Ads.SumCommand
{
    using System;
    using System.ComponentModel;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    internal class IgIoSumReadEntity : IgIoSumEntity
    {
        public IgIoSumReadEntity(uint indexGroup, uint indexOffset, int readLength) : base(indexGroup, indexOffset, readLength, 0)
        {
        }
    }
}

