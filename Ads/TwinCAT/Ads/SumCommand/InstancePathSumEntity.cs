namespace TwinCAT.Ads.SumCommand
{
    using System;
    using System.ComponentModel;
    using TwinCAT.TypeSystem;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    internal class InstancePathSumEntity : SumDataEntity
    {
        public readonly string InstancePath;

        public InstancePathSumEntity(string instancePath, int readLength) : base(readLength, PlcStringConverter.DefaultVariableLength.MarshalSize(instancePath))
        {
            this.InstancePath = instancePath;
        }

        public byte[] GetWriteBytes() => 
            PlcStringConverter.DefaultVariableLength.Marshal(this.InstancePath);
    }
}

