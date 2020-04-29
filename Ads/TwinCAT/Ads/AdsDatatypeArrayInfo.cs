namespace TwinCAT.Ads
{
    using System;
    using System.ComponentModel;
    using System.Text;
    using TwinCAT.Ads.Internal;

    public class AdsDatatypeArrayInfo : IAdsCustomMarshal<AdsDatatypeArrayInfo>
    {
        private int lBound;
        private int elements;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public AdsDatatypeArrayInfo()
        {
        }

        internal AdsDatatypeArrayInfo(int lowerBound, int elements)
        {
            this.lBound = lowerBound;
            this.elements = elements;
        }

        internal AdsDatatypeArrayInfo(long parentEndPosition, Encoding encoding, AdsBinaryReader reader)
        {
            this.Read(parentEndPosition, encoding, reader);
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public void Read(long parentEndPosition, Encoding encoding, AdsBinaryReader reader)
        {
            this.lBound = reader.ReadInt32();
            this.elements = (int) reader.ReadUInt32();
        }

        public int LowerBound =>
            this.lBound;

        public int Elements =>
            this.elements;
    }
}

