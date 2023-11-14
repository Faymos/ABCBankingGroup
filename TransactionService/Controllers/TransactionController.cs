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
        private readonly WalletTransactions _walletTransactions;

        public TransactionController(ILogger<TransactionController> logger, WalletTransactions walletTransactions)
        {
            _logger = logger;
            _walletTransactions = walletTransactions;
        }

        //[HttpPost("deposit")]
        //public async Task<ActionResult> Deposit(string accountNumber, float amount)
        //{
        //    try
        //    {
        //        // Assuming WalletAccount is a class with a Deposit method
        //        var result = new WalletAccount(accountNumber).Deposit(amount);
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest($"Error: {ex.Message}");
        //    }
        //}

        //[HttpPost("withdraw")]
        //public async Task<ActionResult> Withdraw(string accountNumber, float amount)
        //{
        //    try
        //    {
        //        // Assuming WalletAccount is a class with a WithdrawTransactions method
        //        var result = new WalletAccount(accountNumber).WithdrawTransactions(amount);
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest($"Error: {ex.Message}");
        //    }
        //}

        //[HttpPost("transfer")]
        //public async Task<ActionResult> Transfer(string sourceAccountNumber, float amount, string targetAccountNumber)
        //{
        //    try
        //    {
        //        // Assuming WalletAccount is a class with a Transfer method
        //        var result = new WalletAccount(sourceAccountNumber).Transfer(amount, targetAccountNumber);
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest($"Error: {ex.Message}");
        //    }
        //}

        [HttpGet("balance")]
        public async Task<ActionResult> Balance(string accountNumber)
        {
            try
            {
                // Assuming WalletAccount is a class with a GetBalance method
                var result =  _walletTransactions.GetBalance(accountNumber);
               
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        //[HttpGet("statements")]
        //public async Task<ActionResult> Statements(string accountNumber)
        //{
        //    try
        //    {
               
        //        var result = new WalletTransactions(accountNumber).GetStatements();
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest($"Error: {ex.Message}");
        //    }
        //}
    }
}