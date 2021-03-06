﻿namespace TwinCAT.Ads
{
    using System;
    using System.ComponentModel;

    public enum AmsPort
    {
        Router = 1,
        Debugger = 2,
        R0_TComServer = 10,
        R0_TComServerTask = 11,
        R0_TComServer_PL = 12,
        R0_TcDebugger = 20,
        R0_TcDebuggerTask = 0x15,
        R0_LicenseServer = 30,
        Logger = 100,
        EventLog = 110,
        DeviceApplication = 120,
        EventLog_UM = 130,
        EventLog_RT = 0x83,
        EventLogPublisher = 0x84,
        R0_Realtime = 200,
        R0_Trace = 290,
        R0_IO = 300,
        R0_NC = 500,
        R0_NCSAF = 0x1f5,
        R0_NCSVB = 0x1ff,
        R0_NCINSTANCE = 520,
        R0_ISG = 550,
        R0_CNC = 600,
        R0_LINE = 700,
        R0_PLC = 800,
        [Obsolete("Use 'Tc2_Plc1 instead!"), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        PlcRuntime1 = 0x321,
        Tc2_Plc1 = 0x321,
        [Obsolete("Use 'Tc2_Plc2 instead!"), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        PlcRuntime2 = 0x32b,
        Tc2_Plc2 = 0x32b,
        [Obsolete("Use 'Tc2_Plc3 instead!"), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        PlcRuntime3 = 0x335,
        Tc2_Plc3 = 0x335,
        [Obsolete("Use 'Tc2_Plc4 instead!"), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        PlcRuntime4 = 0x33f,
        Tc2_Plc4 = 0x33f,
        R0_RTS = 850,
        CamshaftController = 900,
        R0_CAMTOOL = 950,
        R0_USER = 0x7d0,
        SystemService = 0x2710,
        R3_CTRLPROG = 0x2710,
        R3_SYSCTRL = 0x2711,
        R3_SYSSAMPLER = 0x2774,
        R3_TCPRAWCONN = 0x27d8,
        R3_TCPIPSERVER = 0x27d9,
        R3_SYSMANAGER = 0x283c,
        R3_SMSSERVER = 0x28a0,
        R3_MODBUSSERVER = 0x2904,
        R3_AMSLOGGER = 0x2906,
        [Obsolete("Use R3_XMLDATASERVER instead", false)]
        R3_S7SERVER = 0x2968,
        R3_XMLDATASERVER = 0x2968,
        R3_AUTOCONFIG = 0x29cc,
        R3_PLCCONTROL = 0x2a30,
        R3_FTPCLIENT = 0x2a94,
        R3_NCCTRL = 0x2af8,
        R3_NCINTERPRETER = 0x2cec,
        R3_GSTINTERPRETER = 0x2d50,
        R3_STRECKECTRL = 0x2ee0,
        R3_CAMCTRL = 0x32c8,
        R3_SCOPE = 0x36b0,
        R3_CONDITIONMON = 0x3714,
        R3_SINECH1 = 0x3a98,
        R3_CONTROLNET = 0x3e80,
        R3_OPCSERVER = 0x4268,
        R3_OPCCLIENT = 0x445c,
        R3_MAILSERVER = 0x4650,
        R3_EL60XX = 0x4a38,
        R3_MANAGEMENT = 0x4a9c,
        R3_MIELEHOME = 0x4b00,
        R3_CPLINK3 = 0x4b64,
        R3_VNSERVICE = 0x4c2c,
        USEDEFAULT = 0xffff
    }
}

