namespace TwinCAT.TypeSystem
{
    using System;
    using TwinCAT.Ads;

    [Serializable]
    public class DataTypeException : AdsException
    {
        [NonSerialized]
        public readonly IDataType DataType;

        public DataTypeException(string message, IDataType type) : base(message)
        {
            this.DataType = type;
        }
    }
}

