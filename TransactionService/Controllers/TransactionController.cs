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
        public async Task<ActionResult> Deposit(string accountNumber, decimal amount)
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
        public async Task<ActionResult> Withdraw(string accountNumber, decimal amount)
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
        public async Task<ActionResult> Transfer(string sourceAccountNumber, decimal amount, string targetAccountNumber)
        {
            if (sourceAccountNumber == targetAccountNumber)
            {
                return BadRequest($"Error: source and destination account can not be the same");
            }
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
        public async Task<ActionResult> TransferOtherBank(string sourceAccountNumber, decimal amount, string targetAccountNumber, string bankcode)
        {

            if (sourceAccountNumber == targetAccountNumber)
            {
                return BadRequest($"Error: source and destination account can not be the same");
            }
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

        [HttpGet("statement")]
        public async Task<ActionResult> Statement(string accountNumber)
        {
            try
            {

                var result = await _walletTransactions.GetStatement(accountNumber);
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

        [HttpGet("getaccountName")]
        public async Task<IActionResult> AccountVerification(string accountNumber)
        {
            var result = await _walletTransactions.AccountVerification(accountNumber);
           if (result == null)
            {
                return NotFound();
            }
           return Ok(result);
        }

        [HttpGet("Overdraft")]
        public async Task<ActionResult> Overdraft()
        {
            try
            {
                // Assuming WalletAccount is a class with a GetBalance method
                var result = await _walletTransactions.Overdraft();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}