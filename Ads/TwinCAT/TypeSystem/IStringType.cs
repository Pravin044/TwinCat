namespace TwinCAT.TypeSystem
{
    using System;
    using System.Text;

    public interface IStringType : IDataType, IBitSize
    {
        int Length { get; }

        System.Text.Encoding Encoding { get; }

        bool IsFixedLength { get; }
    }
}

