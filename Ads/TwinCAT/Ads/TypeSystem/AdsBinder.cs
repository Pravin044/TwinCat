namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using TwinCAT.Ads;
    using TwinCAT.TypeSystem;

    internal sealed class AdsBinder : Binder, IAdsBinder, IBinder, IDataTypeResolver
    {
        private AmsAddress _imageBaseAddress;

        internal AdsBinder(AmsAddress imageBaseAddress, IInternalSymbolProvider provider, ISymbolFactory symbolFactory, bool useVirtualInstance) : base(provider, symbolFactory, useVirtualInstance)
        {
            this._imageBaseAddress = imageBaseAddress;
        }

        public AmsAddress ImageBaseAddress =>
            this._imageBaseAddress;
    }
}

