namespace TwinCAT.Ads.SumCommand
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;
    using TwinCAT.TypeSystem;

    public class SumReleaseHandles : SumCommandWrapper<SumWrite>
    {
        private IAdsConnection _connection;
        private uint[] _serverHandles;
        private PrimitiveTypeConverter _converter = PrimitiveTypeConverter.Default;

        public SumReleaseHandles(IAdsConnection connection, uint[] serverHandles)
        {
            this._connection = connection;
            this._serverHandles = serverHandles;
        }

        private IList<SumDataEntity> CreateSumEntityInfos()
        {
            List<SumDataEntity> list = new List<SumDataEntity>();
            foreach (int num2 in this._serverHandles)
            {
                HandleSumEntity item = new HandleSumEntity((uint) num2, 0, 4, this._converter);
                list.Add(item);
            }
            return list;
        }

        public void ReleaseHandles()
        {
            AdsErrorCode[] returnCodes = null;
            AdsErrorCode code = this.TryReleaseHandles(out returnCodes);
            if (base.Failed)
            {
                throw new AdsSumCommandException("SumReleaseHandles failed!", this);
            }
        }

        public AdsErrorCode TryReleaseHandles(out AdsErrorCode[] returnCodes)
        {
            List<byte[]> writeData = new List<byte[]>();
            IList<SumDataEntity> sumEntities = this.CreateSumEntityInfos();
            foreach (HandleSumEntity entity in sumEntities)
            {
                byte[] bytes = BitConverter.GetBytes(entity.Handle);
                writeData.Add(bytes);
            }
            base.innerCommand = new SumWrite(this._connection, sumEntities, TwinCAT.Ads.SumCommand.SumCommand.SumAccessMode.ReleaseHandle);
            return base.innerCommand.TryWriteRaw(writeData, out returnCodes);
        }
    }
}

