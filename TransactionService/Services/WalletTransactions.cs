using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Threading.Tasks;
using System.Transactions;
using TransactionService.Entities;
using TransactionService.Model;

namespace TransactionService.Services
{
    public class WalletTransactions : IWalletTransaction
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
                .Where(u => u.AccountNumber == accountNumber.Trim() && u.DateCreated >= startDate && u.DateCreated <= endDate)
                .ToListAsync();

            return new ResponseData
            {
                Status = HttpStatusCode.OK,
                ResponseMessage = "Successful",
                data = result
            };
        }

        public async Task<string> GetBalance(string accountNumber)
        {
            var result =  _dbContext.customerAccount
                .FirstOrDefault(u => u.AccountNumber == accountNumber)?.CurrentBalance.ToString();

            return result ?? "Account not found";
        }
        public async Task<string> AccountVerification(string accountNumber)
        {
            var result = _dbContext.customerAccount
                .FirstOrDefault(u => u.AccountNumber == accountNumber)?.AccountName.ToString();

            return result ?? null;
        }
        public async Task<ResponseData> Deposit(string accountNumber, decimal amount)
        {
            // Find the existing record in the database
            var existingAccount = await _dbContext.customerAccount
                .FirstOrDefaultAsync(u => u.AccountNumber == accountNumber);

            Transactions transactions = new()
            {
                TransMethod = TransMethod.deposit.ToString(),
                Type = TransType.cr.ToString(),
                AccountNumber = accountNumber,
                Amount = amount,
                BeneficiaryAccount = existingAccount.AccountNumber,
                BeneficiaryBank = "ABC BANKING GROUP ",
                BeneficiaryName = existingAccount.AccountName,
                CustomerAccountId = existingAccount.Id,
                DateCreated = DateTime.UtcNow,
                SenderName = existingAccount.AccountName,
                SessionId = getSessionId(),
                Remarks= "Self Deposit"
            };

            if (existingAccount != null)
            {
                // Update the properties of the existing record with the new values
                existingAccount.CurrentBalance += amount;
                existingAccount.CrAmount += amount;
                existingAccount.TotalBalance += amount;
                existingAccount.DateUpdated = DateTime.UtcNow;
               if( await _dbContext.SaveChangesAsync() > 0)
                {
                    if (await TransactionsDetails(transactions))
                    {
                        return new ResponseData()
                        {
                            ResponseMessage = "Deposit Successful",
                            Status = HttpStatusCode.OK
                        };
                    }
                    else
                    {
                        
                        return null;
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Customer account with account number {accountNumber} not found.");
                }
                
            }
            else
            {
                throw new InvalidOperationException($"Customer account with account number {accountNumber} not found.");
            }

        }
        public async Task<ResponseData> Withdrawal(string accountNumber, decimal amount)
        {
            try
            {
                var result = await _dbContext.customerAccount
               .FirstOrDefaultAsync(u => u.AccountNumber == accountNumber);

                if (viaableTransaction(result.CurrentBalance, amount))
                {
                    Transactions transactions = new()
                    {
                        TransMethod = TransMethod.withdrawal.ToString(),
                        Type = TransType.dr.ToString(),
                        AccountNumber = accountNumber,
                        Amount = amount,
                        CustomerAccountId = result.Id,
                        DateCreated = DateTime.UtcNow,
                        Remarks = "Self Withdrawal",
                        SessionId = getSessionId()
                    };

                    result.CurrentBalance -= amount;
                    result.DrAmount += amount;
                    result.TotalBalance -= amount;
                    result.DateUpdated = DateTime.UtcNow;
                    await _dbContext.SaveChangesAsync();
                       
                    if (await TransactionsDetails(transactions))
                    {
                        return new ResponseData()
                        {
                            ResponseMessage = "withdrawal Successful",
                            Status = HttpStatusCode.OK
                        };
                    }
                    else
                    {

                        return null;
                    }
                                         
                }
                else
                {
                    return new ResponseData
                    {
                        Status = HttpStatusCode.BadRequest,
                        ResponseMessage = "Insufficient balance"
                    };
                }
            }
            catch(Exception ex) 
            {
                throw ex;
            }
        }
        public async Task<ResponseData> Transfer(string accountNumber, string DestinationAccount, decimal amount)
        {
            try
            {
                var result = await _dbContext.customerAccount
               .FirstOrDefaultAsync(u => u.AccountNumber == accountNumber);

                if (viaableTransaction(result.CurrentBalance, amount))
                {
                    var result2 = await _dbContext.customerAccount
               .FirstOrDefaultAsync(u => u.AccountNumber == DestinationAccount);
                    var sessionid = getSessionId();

                    Transactions transactions = new()
                    {
                        TransMethod = TransMethod.transfer.ToString(),
                        Type = TransType.dr.ToString(),
                        AccountNumber = accountNumber,
                        Amount = amount,
                        CustomerAccountId = result.Id,
                        DateCreated = DateTime.UtcNow,
                        SenderAccount = result.AccountNumber,
                        SenderBank= "ABC Banking Group",
                        SenderName = result.AccountName,
                        BeneficiaryAccount= result2.AccountNumber,
                        BeneficiaryBank = "ABC Banking Group",
                        BeneficiaryName = result2.AccountName,
                        Remarks = $"transfer to  {result2.AccountName}",
                        SessionId = sessionid
                    };

                    result.CurrentBalance -= amount;
                    result.DrAmount += amount;
                    result.TotalBalance -= amount;
                    result.DateUpdated = DateTime.UtcNow;

                    await _dbContext.SaveChangesAsync();

                    if (await TransactionsDetails(transactions))
                    {
                        return await SameBankDeposit(DestinationAccount, amount,accountNumber, result.AccountName, sessionid);
                    }
                    else
                    {

                        return null;
                    }

                }
                else
                {
                    return new ResponseData
                    {
                        Status = HttpStatusCode.BadRequest,
                        ResponseMessage = "Insufficient balance"
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<ResponseData> SameBankDeposit(string accountNumber, decimal amount, string senderaccount, string sendername, string sessionId)
        {
            // Find the existing record in the database
            var existingAccount = await _dbContext.customerAccount
                .FirstOrDefaultAsync(u => u.AccountNumber == accountNumber);

            Transactions transactions = new()
            {
                TransMethod = TransMethod.transfer.ToString(),
                Type = TransType.cr.ToString(),
                AccountNumber = existingAccount.AccountNumber,
                Amount = amount,
                CustomerAccountId = existingAccount.Id,
                DateCreated = DateTime.UtcNow,
                SenderAccount = senderaccount,
                SenderBank = "ABC Banking Group",
                SenderName = sendername,
                BeneficiaryAccount = existingAccount.AccountNumber,
                BeneficiaryBank = "ABC Banking Group",
                BeneficiaryName = existingAccount.AccountName,
                Remarks = $"transfer from  {sendername}",
                SessionId = sessionId
            };

            if (existingAccount != null)
            {
                
                existingAccount.CurrentBalance += amount;
                existingAccount.CrAmount += amount;
                existingAccount.TotalBalance += amount;
                existingAccount.DateUpdated = DateTime.UtcNow;
                if (await _dbContext.SaveChangesAsync() > 0)
                {
                    if (await TransactionsDetails(transactions))
                    {
                        return new ResponseData()
                        {
                            ResponseMessage = "Successful",
                            Status = HttpStatusCode.OK
                        };
                    }
                    else
                    {

                        return null;
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Customer account with account number {accountNumber} not found.");
                }

            }
            else
            {
                throw new InvalidOperationException($"Customer account with account number {accountNumber} not found.");
            }

        }
        public Task<ResponseData> TransferToOtherBank(string accountNumber, string DestinationAccount, string bankCode, decimal amount)
        {
            throw new NotImplementedException();
        }

        private async Task<bool> TransactionsDetails(Transactions transactions)
        {
            try
            {
                _dbContext.transactions.Add(transactions);
                if (await _dbContext.SaveChangesAsync() > 0)
                {
                    return true;
                }
                return false;
            }
             catch (Exception ex)
            {
                throw ex;
            }
        }

        private  bool viaableTransaction(decimal currentBalance, decimal amount)
        {           
            if (currentBalance > amount)
            {
                return true;
            }
            return false;
        }

        private static string getSessionId()
        {
            Random rxd = new();
            return DateTime.Now.ToString("yyMMddHHmmss") + rxd.Next(9999).ToString();
        }
    }
}
