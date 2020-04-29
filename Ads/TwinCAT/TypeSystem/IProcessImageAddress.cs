namespace TwinCAT.TypeSystem
{
    using System;

    public interface IProcessImageAddress : IBitSize
    {
        uint IndexGroup { get; }

        uint IndexOffset { get; }
    }
}

