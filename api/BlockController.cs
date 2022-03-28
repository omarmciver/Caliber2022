using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.BlockChain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace api
{
    [ApiController]
    [Route("[controller]")]
    public class BlockController : ControllerBase
    {
        private readonly ILogger<BlockController> _logger = default!;
        public BlockController(ILogger<BlockController> logger)
        {
            _logger = logger;
        }

        [HttpGet("Mine")]
        public async Task<ActionResult<string>> Mine(int numberOfTransactions)
        {
            // === Build a Block with random records
            _logger.LogInformation($"Building {numberOfTransactions} transactions in a block.");
            var transactions = new List<BlockRecordBase>();
            for (int i = 0; i < numberOfTransactions; i++)
            {
                transactions.Add(
                    new BasicTransaction(
                        from: Guid.NewGuid().ToString(),
                        to: Guid.NewGuid().ToString(),
                        amount: new Random().NextDouble())
                    );
            }
            var block = new Block<BasicTransaction>(timeStamp: DateTime.Now, records: transactions);

            // === Mine the block (calculate the hash)
            _logger.LogInformation($"Mining the block...");
            await block.MineBlock(proofOfWorkDifficulty: 5, minerName: "Omar McIver");

            // === Discover what the hash was and return it
            var mineDetails = $"{block.Miner} mined {block.Records.Count} records in {block.TimeToMine.TotalSeconds} seconds. Hash: {block.Hash}";
            _logger.LogInformation(mineDetails);
            return Ok(mineDetails);
        }
    }
}
