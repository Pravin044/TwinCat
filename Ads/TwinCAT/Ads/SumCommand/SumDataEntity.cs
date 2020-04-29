namespace TwinCAT.Ads.SumCommand
{
    using System;
    using System.ComponentModel;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public abstract class SumDataEntity
    {
        protected int readLength;
        protected int writeLength;

        protected SumDataEntity()
        {
        }

        protected SumDataEntity(int readLength, int writeLength)
        {
            this.readLength = readLength;
            this.writeLength = writeLength;
        }

        internal void SetWriteLength(int length)
        {
            this.writeLength = length;
        }

        public int ReadLength =>
            this.readLength;

        public int WriteLength =>
            this.writeLength;
    }
}

