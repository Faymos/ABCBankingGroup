using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Services;

namespace TransactionService.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [EnableCors]
    [Authorize]
    public class TransactionController : ControllerBase
    {
        
        private readonly ILogger<TransactionController> _logger;
        private readonly IWalletTransaction _walletTransactions;

        public TransactionController(ILogger<TransactionController> logger, IWalletTransaction walletTransactions)
        {
            _logger = logger;
            _walletTransactions = walletTransactions;
        }

        [HttpPost("deposit")]
        public async Task<ActionResult> Deposit(string accountNumber, float amount)
        {
            try
            {
                
                var result = await _walletTransactions.Deposit(accountNumber, amount);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost("withdraw")]
        public async Task<ActionResult> Withdraw(string accountNumber, float amount)
        {
            try
            {
                
                var result = await _walletTransactions.Withdrawal(accountNumber, amount);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost("transfer")]
        public async Task<ActionResult> Transfer(string sourceAccountNumber, float amount, string targetAccountNumber)
        {
            try
            {
               
                var result = await _walletTransactions.Transfer(sourceAccountNumber,targetAccountNumber, amount);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
        
        [HttpPost("InterBanktransfer")]
        public async Task<ActionResult> TransferOtherBank(string sourceAccountNumber, float amount, string targetAccountNumber, string bankcode)
        {
            try
            {

                var result = await _walletTransactions.TransferToOtherBank(sourceAccountNumber, targetAccountNumber,bankcode, amount);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpGet("balance")]
        public async Task<ActionResult> Balance(string accountNumber)
        {
            try
            {
                // Assuming WalletAccount is a class with a GetBalance method
                var result = await _walletTransactions.GetBalance(accountNumber);
               
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpGet("statements")]
        public async Task<ActionResult> Statements(string accountNumber, DateTime startdate, DateTime enddate)
        {
            try
            {

                var result = await _walletTransactions.GetStatements(accountNumber, startdate, enddate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}