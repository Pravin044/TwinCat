namespace TwinCAT.ValueAccess
{
    using System;
    using TwinCAT.Ads;

    [Serializable]
    public class ValueAccessorException : AdsException
    {
        [NonSerialized]
        public readonly IAccessorRawValue Accessor;

        public ValueAccessorException(string message, IAccessorRawValue accessor) : base(message)
        {
            this.Accessor = accessor;
        }

        public ValueAccessorException(IAccessorRawValue accessor, Exception innerException) : this($"Value Accessor '{accessor.ToString()}' failed!", accessor, innerException)
        {
        }

        public ValueAccessorException(string message, IAccessorRawValue accessor, Exception innerException) : base(message, innerException)
        {
            this.Accessor = accessor;
        }
    }
}

