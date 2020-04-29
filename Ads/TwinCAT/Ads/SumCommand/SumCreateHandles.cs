namespace TwinCAT.Ads.SumCommand
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;

    public class SumCreateHandles : SumCommandWrapper<SumReadWrite>
    {
        private IAdsConnection _connection;
        private string[] _instancePaths;

        public SumCreateHandles(IAdsConnection connection, IList<string> instancePaths)
        {
            this._connection = connection;
            this._instancePaths = Enumerable.ToArray<string>(instancePaths);
        }

        public SumCreateHandles(IAdsConnection connection, string[] instancePaths)
        {
            this._connection = connection;
            this._instancePaths = instancePaths;
        }

        public uint[] CreateHandles()
        {
            uint[] handles = null;
            AdsErrorCode[] codeArray;
            string[] instancePaths = null;
            AdsErrorCode code = this.TryCreateHandles(out instancePaths, out handles, out codeArray);
            if (base.Failed)
            {
                throw new AdsSumCommandException("SumGetHandles failed!", this);
            }
            return handles;
        }

        private IList<SumDataEntity> CreateSumEntityInfos()
        {
            List<SumDataEntity> list = new List<SumDataEntity>();
            foreach (string str in this._instancePaths)
            {
                InstancePathSumEntity item = new InstancePathSumEntity(str, 4);
                list.Add(item);
            }
            return list;
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public AdsErrorCode TryCreateHandles(out ISumHandleCollection handles)
        {
            IList<SumDataEntity> sumEntities = this.CreateSumEntityInfos();
            handles = null;
            AdsErrorCode[] returnCodes = null;
            List<byte[]> writeData = new List<byte[]>();
            IList<byte[]> readData = null;
            foreach (InstancePathSumEntity entity in sumEntities)
            {
                writeData.Add(entity.GetWriteBytes());
            }
            base.innerCommand = new SumReadWrite(this._connection, sumEntities, TwinCAT.Ads.SumCommand.SumCommand.SumAccessMode.AquireHandleByName);
            AdsErrorCode code = base.innerCommand.TryReadWriteRaw(writeData, out readData, out returnCodes);
            if (code == AdsErrorCode.NoError)
            {
                handles = new SumHandleList();
                for (int i = 0; i < sumEntities.Count; i++)
                {
                    uint handle = 0;
                    if (returnCodes[i] == AdsErrorCode.NoError)
                    {
                        handle = BitConverter.ToUInt32(readData[i], 0);
                    }
                    handles.Add(new SumHandleInstancePathEntry(this._instancePaths[i], handle, returnCodes[i]));
                }
            }
            return code;
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never), Obsolete("Use the TryCreateHandles(out string[] instancePaths, out uint[] handles, out AdsErrorCode[] returnCodes) overload instead!")]
        public AdsErrorCode TryCreateHandles(out uint[] handles, out AdsErrorCode[] returnCodes)
        {
            ISumHandleCollection handles2 = null;
            AdsErrorCode code = this.TryCreateHandles(out handles2);
            returnCodes = new AdsErrorCode[handles2.Count];
            handles = new uint[handles2.Count];
            for (int i = 0; i < handles2.Count; i++)
            {
                handles[i] = handles2[i].Handle;
                returnCodes[i] = handles2[i].ErrorCode;
            }
            return code;
        }

        public AdsErrorCode TryCreateHandles(out string[] instancePaths, out uint[] handles, out AdsErrorCode[] returnCodes)
        {
            ISumHandleCollection handles2 = null;
            AdsErrorCode code = this.TryCreateHandles(out handles2);
            returnCodes = new AdsErrorCode[handles2.Count];
            handles = new uint[handles2.Count];
            for (int i = 0; i < handles2.Count; i++)
            {
                handles[i] = handles2[i].Handle;
                returnCodes[i] = handles2[i].ErrorCode;
            }
            instancePaths = this._instancePaths;
            return code;
        }
    }
}

