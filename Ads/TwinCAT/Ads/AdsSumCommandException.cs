namespace TwinCAT.Ads
{
    using System;
    using System.Runtime.Serialization;
    using TwinCAT.Ads.SumCommand;

    [Serializable]
    public class AdsSumCommandException : AdsErrorException
    {
        [NonSerialized]
        private AdsErrorCode[] entityExceptions;
        [NonSerialized]
        private ISumCommand command;

        public AdsSumCommandException(string message, ISumCommand command) : base(message, command.Result)
        {
            this.command = command;
            this.entityExceptions = command.SubResults;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("SumCommand", this.SumCommand);
        }

        public ISumCommand SumCommand =>
            this.command;
    }
}

