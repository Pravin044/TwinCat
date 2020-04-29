namespace TwinCAT.Ads.Internal
{
    using System;

    internal class QueueElement
    {
        public int handle;
        public long timeStamp;
        public byte[] data;

        public QueueElement()
        {
        }

        public QueueElement(int handle, long timeStamp, byte[] data)
        {
            this.handle = handle;
            this.timeStamp = timeStamp;
            this.data = data;
        }
    }
}

