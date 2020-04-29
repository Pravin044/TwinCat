namespace TwinCAT.Ads.Internal
{
    using System;

    internal class CycleListEntry
    {
        public VariableInfo variable;
        public int handle;
        public int transMode;
        public byte[] data;
    }
}

