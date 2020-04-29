namespace TwinCAT.Ads.TypeSystem
{
    using TwinCAT.Ads;
    using TwinCAT.TypeSystem;

    public interface IAdsSymbol : ISymbol, IAttributedInstance, IInstance, IBitSize, IProcessImageAddress
    {
        AmsAddress ImageBaseAddress { get; }
    }
}

