using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace api.BlockChain
{
    [Serializable]
    [DataContract]
    public class BasicTransaction : BlockRecordBase
    {
        [DataMember]
        public string From { get; set; }
        [DataMember]
        public string To { get; set; }
        [DataMember]
        public double Amount { get; set; }
        
        public BasicTransaction()
        {

        }
        public BasicTransaction(string from, string to, double amount)
        {
            From = from;
            RecordKey = To = to;
            Amount = amount;
        }
    }
}
