namespace TwinCAT.Ads
{
    using System;
    using System.Runtime.CompilerServices;
    using TwinCAT;
    using TwinCAT.Ads.ValueAccess;
    using TwinCAT.ValueAccess;

    public class SymbolLoaderSettings : ISymbolLoaderSettings
    {
        private ValueCreationMode _creationMode;

        public SymbolLoaderSettings(TwinCAT.SymbolsLoadMode loadMode) : this(loadMode, TwinCAT.Ads.ValueAccess.ValueAccessMode.IndexGroupOffsetPreferred)
        {
        }

        public SymbolLoaderSettings(TwinCAT.SymbolsLoadMode loadMode, TwinCAT.Ads.ValueAccess.ValueAccessMode valueAccess)
        {
            this._creationMode = ValueCreationMode.Default;
            this.SymbolsLoadMode = loadMode;
            this.ValueAccessMode = valueAccess;
            this.NonCachedArrayElements = true;
            this.ValueCreation = ValueCreationMode.Default;
        }

        public SymbolLoaderSettings(TwinCAT.SymbolsLoadMode loadMode, ValueCreationMode valueCreation, TwinCAT.Ads.ValueAccess.ValueAccessMode valueAccess)
        {
            this._creationMode = ValueCreationMode.Default;
            this.SymbolsLoadMode = loadMode;
            this.ValueAccessMode = valueAccess;
            this._creationMode = valueCreation;
            this.NonCachedArrayElements = true;
        }

        public TwinCAT.SymbolsLoadMode SymbolsLoadMode { get; set; }

        public TwinCAT.Ads.ValueAccess.ValueAccessMode ValueAccessMode { get; set; }

        public bool NonCachedArrayElements { get; set; }

        public bool AutomaticReconnection { get; set; }

        public ValueCreationMode ValueCreation
        {
            get => 
                this._creationMode;
            set => 
                (this._creationMode = value);
        }

        public static SymbolLoaderSettings Default =>
            new SymbolLoaderSettings(TwinCAT.SymbolsLoadMode.VirtualTree, TwinCAT.Ads.ValueAccess.ValueAccessMode.IndexGroupOffsetPreferred);

        public static SymbolLoaderSettings DefaultDynamic =>
            new SymbolLoaderSettings(TwinCAT.SymbolsLoadMode.DynamicTree, ValueCreationMode.Default, TwinCAT.Ads.ValueAccess.ValueAccessMode.IndexGroupOffsetPreferred);
    }
}

