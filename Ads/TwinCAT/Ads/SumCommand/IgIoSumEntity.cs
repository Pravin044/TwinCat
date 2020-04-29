namespace TwinCAT.Ads.SumCommand
{
    using System;
    using System.ComponentModel;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    internal class IgIoSumEntity : SumDataEntity
    {
        public readonly uint IndexGroup;
        public readonly uint IndexOffset;

        public IgIoSumEntity(uint indexGroup, uint indexOffset, int readLength, int writeLength) : base(readLength, writeLength)
        {
            this.IndexGroup = indexGroup;
            this.IndexOffset = indexOffset;
        }
    }
}

