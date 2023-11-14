using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Threading.Tasks;
using TransactionService.Entities;
using TransactionService.Model;

namespace TransactionService.Services
{
    public class WalletTransactions
    {
        private readonly TransactionDbContext _dbContext;

        public WalletTransactions(TransactionDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CustomerAccount> GetCustomerAccount(string accountNumber)
        {
            return await _dbContext.customerAccount
                .FirstOrDefaultAsync(u => u.AccountNumber == accountNumber);
        }

        public async Task<ResponseData> GetStatements(string accountNumber, DateTime startDate, DateTime endDate)
        {
            var result = await _dbContext.transactions
                .Where(u => u.AccountNumber == accountNumber && u.DateCreated >= startDate && u.DateCreated <= endDate)
                .ToListAsync();

            return new ResponseData
            {
                Status = HttpStatusCode.OK,
                ResponseMessage = "Successful",
                data = result
            };
        }

        public string GetBalance(string accountNumber)
        {
            var result = _dbContext.customerAccount
                .FirstOrDefault(u => u.AccountNumber == accountNumber)?.CurrentBalance.ToString();

            return result ?? "Account not found";
        }
    }
}
