namespace TwinCAT.Ads
{
    using System;
    using System.Diagnostics;
    using System.IO;

    [DebuggerDisplay("Length = {Length} Position = {Position}")]
    public class AdsStream : MemoryStream
    {
        protected int origin;

        public AdsStream()
        {
        }

        public AdsStream(int length) : base(new byte[length], 0, length, true, true)
        {
        }

        public AdsStream(byte[] buffer) : base(buffer, 0, buffer.Length, true, true)
        {
        }

        public AdsStream(byte[] buffer, int offset, int length) : base(buffer, offset, length, true, true)
        {
            this.origin = offset;
        }

        internal int Origin =>
            this.origin;
    }
}

