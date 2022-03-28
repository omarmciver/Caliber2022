using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace api.BlockChain
{
    [Serializable]
    [DataContract]
    public abstract class BlockRecordBase
    {
        [DataMember]
        public string RecordKey { get; set; }
        [DataMember]
        public DateTime RecordedDate { get; set; }
        [DataMember]
        public string UniqueID { get; set; }

        public BlockRecordBase()
        {
            UniqueID = Guid.NewGuid().ToString();
        }
    }
}
