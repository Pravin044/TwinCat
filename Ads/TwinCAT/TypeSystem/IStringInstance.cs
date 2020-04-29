namespace TwinCAT.TypeSystem
{
    using System;
    using System.Text;

    public interface IStringInstance : ISymbol, IAttributedInstance, IInstance, IBitSize
    {
        System.Text.Encoding Encoding { get; }

        bool IsFixedLength { get; }
    }
}

