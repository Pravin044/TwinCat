namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using TwinCAT;
    using TwinCAT.Ads;
    using TwinCAT.Ads.Internal;
    using TwinCAT.Ads.ValueAccess;
    using TwinCAT.TypeSystem;
    using TwinCAT.ValueAccess;

    public class SymbolLoaderFactory
    {
        public static ISymbolLoader Create(IConnection connection, ISymbolLoaderSettings settings)
        {
            SymbolUploadInfo symbolsInfo = readSymbolUploadInfo((IAdsConnection) connection);
            return new AdsSymbolLoader((IAdsConnection) connection, (SymbolLoaderSettings) settings, createValueAccessor((IAdsConnection) connection, (SymbolLoaderSettings) settings), ((IAdsConnection) connection).Session, symbolsInfo);
        }

        internal static IAccessorValue createValueAccessor(IAdsConnection connection, SymbolLoaderSettings settings)
        {
            IAccessorValue value2 = null;
            AdsValueAccessor inner = null;
            IAccessorValueFactory valueFactory = null;
            if (settings.SymbolsLoadMode == SymbolsLoadMode.DynamicTree)
            {
                valueFactory = new DynamicValueFactory(settings.ValueCreation);
                inner = new AdsValueAccessor(connection, settings.ValueAccessMode, valueFactory, NotificationSettings.Default);
                value2 = new DynamicValueAccessor(inner, valueFactory, settings.ValueCreation);
            }
            else
            {
                valueFactory = new ValueFactory(settings.ValueCreation);
                inner = new AdsValueAccessor(connection, settings.ValueAccessMode, valueFactory, NotificationSettings.Default);
                value2 = inner;
            }
            IAccessorValueFactory2 factory2 = valueFactory as IAccessorValueFactory2;
            if (valueFactory != null)
            {
                factory2.SetValueAccessor(value2);
            }
            inner.AutomaticReconnection = settings.AutomaticReconnection;
            return value2;
        }

        internal static SymbolUploadInfo readSymbolUploadInfo(IAdsConnection connection)
        {
            SymbolUploadInfo uploadInfo = null;
            AdsErrorCode adsErrorCode = AdsSymbolLoader.loadUploadInfo(connection, TimeSpan.FromMilliseconds((double) connection.Timeout), out uploadInfo);
            if (adsErrorCode != AdsErrorCode.NoError)
            {
                AdsErrorException ex = AdsErrorException.Create(adsErrorCode);
                Module.Trace.TraceWarning("Could not load Symbol Upload info", ex);
                uploadInfo = new SymbolUploadInfo();
            }
            return uploadInfo;
        }
    }
}

