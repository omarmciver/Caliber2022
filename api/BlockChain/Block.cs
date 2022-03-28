using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace api.BlockChain
{
    [Serializable]
    public class Block<IBlockRecord>
    {
        public readonly DateTime TimeStamp;
        public long Nonce { get; set; }
        /// <summary>
        /// This is a unique identifier set when a block is created. It should never form part of the Hash as each node will have a different value.
        /// </summary>
        public string BlockIdentifier { get; set; }
        public Int32 BlockHeight { get; set; }
        /// <summary>
        /// This is a record of the time to mine this block. It should never form part of the Hash as each node will have a different value.
        /// </summary>
        public TimeSpan TimeToMine { get; set; }
        /// <summary>
        /// This is a name to identifier who mined the block. It should never form part of the Hash as each node will have a different value.
        /// </summary>
        public string Miner { get; set; }
        public string PreviousHash { get; set; }
        private string MerkleRootHash { get; set; }
        public List<BlockRecordBase> Records { get; set; }
        public string Hash { get; private set; }

        public string CollectionName => "Blocks";

        public string PartitionKey => "BlockIdentifier";

        /// <summary>
        /// Parameterless constructor only used for deserialization. Do not call otherwise.
        /// </summary>
        public Block()
        {

        }
        public Block(DateTime timeStamp, List<BlockRecordBase> records, string previousHash = "")
        {
            TimeStamp = timeStamp;
            Nonce = 0;
            Records = records;
            PreviousHash = previousHash;
            Hash = CreateHash();
            BlockIdentifier = Guid.NewGuid().ToString();
        }

        public async Task MineBlock(int proofOfWorkDifficulty, string minerName)
        {
            await Task.Run(() =>
            {
                lock (Records)
                {
                    string hashValidationTemplate = new String('0', proofOfWorkDifficulty);
                    Miner = minerName;

                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    do
                    {
                        Nonce++;
                        //We use the private CreateHashWhileMining method so the MerkleTree isn't recalcualted everytime we want to generate a hash.
                        Hash = CreateHashWhileMining();
                    }
                    while (Hash.Substring(0, proofOfWorkDifficulty) != hashValidationTemplate);
                    sw.Stop();
                    TimeToMine = sw.Elapsed;
                }
            });
        }

        /// <summary>
        /// This method forces the MerkleHash to be recalculated based upon the current transactions. Then creates the hash of the block.
        /// </summary>
        /// <returns></returns>
        public string CreateHash()
        {
            MerkleRootHash = null;
            return CreateHashWhileMining();
        }

        /// <summary>
        /// This method generates the MerkleHash once based upon the current transactions. Then repeatedly creates the hash of the block.
        /// </summary>
        /// <returns></returns>
        private string CreateHashWhileMining()
        {
            if (MerkleRootHash == null)
                MerkleRootHash = GenerateMerkleRootHash(Records);

            using (System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create())
            {
                string rawData = BlockHeight + PreviousHash + TimeStamp + MerkleRootHash + Nonce;
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                //return Encoding.Default.GetString(bytes);
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < bytes.Length; i++)
                {
                    sBuilder.Append(bytes[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
        }

        public static string TestGenerateHash(List<BlockRecordBase> Records, int BlockHeight, string PreviousHash, DateTime TimeStamp, long Nonce)
        {
            string merkleRoothash = GenerateMerkleRootHash(Records);
            using (System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create())
            {
                string rawData = BlockHeight + PreviousHash + TimeStamp + merkleRoothash + Nonce;
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                //return Encoding.Default.GetString(bytes);
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < bytes.Length; i++)
                {
                    sBuilder.Append(bytes[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
        }

        private static String GenerateMerkleRootHash(List<BlockRecordBase> trans)
        {
            return BuildMerkleRoot(trans.Select(x => SerializeToXml(x)).ToList());
        }

        private static String SerializeToXml(object o)
        {
            XmlSerializer x = new XmlSerializer(o.GetType());
            using (StringWriter sw = new StringWriter())
            {
                x.Serialize(sw, o);
                return sw.ToString();
            }
        }

        private static string BuildMerkleRoot(IList<string> merkelLeaves)
        {
            if (merkelLeaves == null || !merkelLeaves.Any())
                return string.Empty;

            if (merkelLeaves.Count() == 1)
            {
                return merkelLeaves.First();
            }

            if (merkelLeaves.Count() % 2 > 0)
            {
                merkelLeaves.Add(merkelLeaves.Last());
            }

            var merkleBranches = new List<string>();

            for (int i = 0; i < merkelLeaves.Count(); i += 2)
            {
                var leafPair = string.Concat(merkelLeaves[i], merkelLeaves[i + 1]);
                //double hash
                merkleBranches.Add(GetHash(GetHash(leafPair)));
            }
            return BuildMerkleRoot(merkleBranches);
        }

        private static String GetHash(string stringValue)
        {
            using (System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(stringValue));
                return Encoding.Default.GetString(bytes);
            }
        }

        public List<BlockRecordBase> GetBlockRecords(string byRecordKey = null)
        {
            List<BlockRecordBase> results = new List<BlockRecordBase>();
            if (String.IsNullOrEmpty(byRecordKey))
                results.AddRange(this.Records);
            else
                results.AddRange(this.Records.Where(x => x.RecordKey.Equals(byRecordKey, StringComparison.OrdinalIgnoreCase)));
            return results;
        }

        public List<BlockRecordBase> GetBlockRecords(Type recordType)
        {
            List<BlockRecordBase> results = new List<BlockRecordBase>();
            results.AddRange(this.Records.Where(x => x.GetType().Equals(recordType)));
            return results;
        }
    }
}
