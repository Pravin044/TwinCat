namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Collections.Generic;

    internal class CycleTableEntry
    {
        public uint lastRead;
        public uint timerCount;
        public List<CycleListEntry> cycleList;
    }
}

