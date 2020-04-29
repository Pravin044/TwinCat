namespace TwinCAT.Ads.TypeSystem
{
    using TwinCAT.Ads;
    using TwinCAT.TypeSystem;

    internal interface IAdsBinder : IBinder, IDataTypeResolver
    {
        AmsAddress ImageBaseAddress { get; }
    }
}

