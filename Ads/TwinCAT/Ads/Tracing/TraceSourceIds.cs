namespace TwinCAT.Ads.Tracing
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue"), SuppressMessage("Microsoft.Naming", "CA1717:OnlyFlagsEnumsShouldHavePluralNames")]
    internal enum TraceSourceIds
    {
        ADS = 1,
        AdsSymbols = 2,
        AdsSession = 3,
        VSX = 100,
        AutomationInterface = 110,
        Core = 0x2719,
        Utilities = 0x271a,
        PlugIns = 0x271b,
        Command = 0x271c,
        Communication = 0x271d,
        SystemService = 0x2724,
        SystemManager = 0x272e,
        SystemManagerRCW = 0x272f,
        SystemManagerAdapter = 0x2730,
        PlcControl = 0x2738,
        UIFramework = 0x2742,
        GraphicalEditor = 0x2743,
        EcDescriptions = 0x274c,
        PlugInFramework = 0x2756,
        ContextService = 0x2757,
        DataCore = 0x2760,
        DistributedSystems = 0x276a,
        OpcUa = 0x276b,
        Management = 0x2774,
        Application = 0x7d0
    }
}

