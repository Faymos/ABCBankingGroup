using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Data;
using System.Net;
using System.Threading.Tasks;
using System.Transactions;
using TransactionService.Entities;
using TransactionService.Model;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace TransactionService.Services
{
    public class WalletTransactions : IWalletTransaction
    {
        private readonly TransactionDbContext _dbContext;
        private readonly ILogger<WalletTransactions> _logger;

        public WalletTransactions(TransactionDbContext dbContext, ILogger<WalletTransactions> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<CustomerAccount> GetCustomerAccount(string accountNumber)
        {
            return await _dbContext.customerAccount
                .FirstOrDefaultAsync(u => u.AccountNumber == accountNumber);
        }

        public async Task<ResponseData> GetStatements(string accountNumber, DateTime startDate, DateTime endDate)
        {
            using (var connection = new SqlConnection(_dbContext.Database.GetConnectionString()))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "[dbo].[sp_Transactions]";
                command.CommandType = CommandType.StoredProcedure;

                // Add parameters
                command.Parameters.AddWithValue("@Status", 2);
                command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                command.Parameters.AddWithValue("@StartDate", startDate);
                command.Parameters.AddWithValue("@EndDate", endDate);

                try
                {
                    await connection.OpenAsync();
                    // Use ExecuteReaderAsync to read the result set
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var resultData = new List<Transactions>();

                        while (await reader.ReadAsync())
                        {

                            var transaction = new Transactions
                            {

                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                DateCreated = reader.GetDateTime(reader.GetOrdinal("DateCreated")),
                                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                                AccountNumber = reader.GetString(reader.GetOrdinal("AccountNumber")),
                                SenderName = reader.IsDBNull(reader.GetOrdinal("SenderName")) ? null : reader.GetString(reader.GetOrdinal("SenderName")),
                                SenderAccount = reader.IsDBNull(reader.GetOrdinal("SenderAccount")) ? null : reader.GetString(reader.GetOrdinal("SenderAccount")),
                                SenderBank = reader.IsDBNull(reader.GetOrdinal("SenderBank")) ? null : reader.GetString(reader.GetOrdinal("SenderBank")),
                                Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks")) ? null : reader.GetString(reader.GetOrdinal("Remarks")),
                                BeneficiaryName = reader.IsDBNull(reader.GetOrdinal("BeneficiaryName")) ? null : reader.GetString(reader.GetOrdinal("BeneficiaryName")),
                                BeneficiaryAccount = reader.IsDBNull(reader.GetOrdinal("BeneficiaryAccount")) ? null : reader.GetString(reader.GetOrdinal("BeneficiaryAccount")),
                                BeneficiaryBank = reader.IsDBNull(reader.GetOrdinal("BeneficiaryBank")) ? null : reader.GetString(reader.GetOrdinal("BeneficiaryBank")),
                                Type = reader.GetString(reader.GetOrdinal("Type")),
                                TransMethod = reader.GetString(reader.GetOrdinal("TransMethod")),
                                SessionId = reader.GetString(reader.GetOrdinal("SessionId")),
                                CustomerAccountId = reader.GetInt32(reader.GetOrdinal("CustomerAccountId"))


                            };

                            resultData.Add(transaction);
                        }

                        return new ResponseData
                        {
                            Status = HttpStatusCode.OK,
                            ResponseMessage = "Successful",
                            data = resultData
                        };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An error occurred: {ex.Message}");
                    throw;
                }
            }
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

                if (viaableTransaction(result.CurrentBalance, amount,result.Id))
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

                if (viaableTransaction(result.CurrentBalance, amount, result.Id))
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
                            ResponseMessage = "Transfer Successful",
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

        private async Task<bool> TransactionsDetails(Transactions transaction)
        {
            _logger.LogInformation($"Trasactions:::: {DateTime.UtcNow} :::: {JsonConvert.SerializeObject(transaction) }");


            using (var connection = new SqlConnection(_dbContext.Database.GetConnectionString()))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "sp_Transactions";
                command.CommandType = CommandType.StoredProcedure;

                // Add parameters
                command.Parameters.AddWithValue("@status", 3);
                command.Parameters.AddWithValue("@AccountNumber", transaction.AccountNumber);
                command.Parameters.AddWithValue("@Amount", transaction.Amount);
                command.Parameters.AddWithValue("@DateCreated", transaction.DateCreated);
                command.Parameters.AddWithValue("@SenderName", transaction.SenderName);
                command.Parameters.AddWithValue("@SenderAccount", transaction.SenderAccount);
                command.Parameters.AddWithValue("@SenderBank", transaction.SenderBank);
                command.Parameters.AddWithValue("@Remarks", transaction.Remarks);
                command.Parameters.AddWithValue("@BeneficiaryName", transaction.BeneficiaryName);
                command.Parameters.AddWithValue("@BeneficiaryAccount", transaction.BeneficiaryAccount);
                command.Parameters.AddWithValue("@BeneficiaryBank", transaction.BeneficiaryBank);
                command.Parameters.AddWithValue("@Type", transaction.Type);
                command.Parameters.AddWithValue("@TransMethod", transaction.TransMethod);
                command.Parameters.AddWithValue("@SessionId", transaction.SessionId);
                command.Parameters.AddWithValue("@CustomerAccountId", transaction.CustomerAccountId);

                try
                {
                    await connection.OpenAsync();
                   if (await command.ExecuteNonQueryAsync() > 0)
                    {
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An error occurred: {ex.Message}");
                    throw;
                }

            }
        }
        private  bool viaableTransaction(decimal currentBalance, decimal amount, int id)
        {           
            if (currentBalance > amount)
            {
                return true;
            }
            else if(overDaftAmount(id))
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

        public async Task<ResponseData> GetStatement(string accountNumber)
        {
            using (var connection = new SqlConnection(_dbContext.Database.GetConnectionString()))
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "[dbo].[sp_Transactions]";
                command.CommandType = CommandType.StoredProcedure;

                // Add parameters
                command.Parameters.AddWithValue("@Status", 1);
                command.Parameters.AddWithValue("@AccountNumber", accountNumber);

                try
                {
                    await connection.OpenAsync();

                    // Use ExecuteReaderAsync to read the result set
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var resultData = new List<Transactions>(); 

                        while (await reader.ReadAsync())
                        {
                            
                            var transaction = new Transactions
                            {

                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                DateCreated = reader.GetDateTime(reader.GetOrdinal("DateCreated")),
                                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                                AccountNumber = reader.GetString(reader.GetOrdinal("AccountNumber")),
                                SenderName = reader.IsDBNull(reader.GetOrdinal("SenderName")) ? null : reader.GetString(reader.GetOrdinal("SenderName")),
                                SenderAccount = reader.IsDBNull(reader.GetOrdinal("SenderAccount")) ? null : reader.GetString(reader.GetOrdinal("SenderAccount")),
                                SenderBank = reader.IsDBNull(reader.GetOrdinal("SenderBank")) ? null : reader.GetString(reader.GetOrdinal("SenderBank")),
                                Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks")) ? null : reader.GetString(reader.GetOrdinal("Remarks")),
                                BeneficiaryName = reader.IsDBNull(reader.GetOrdinal("BeneficiaryName")) ? null : reader.GetString(reader.GetOrdinal("BeneficiaryName")),
                                BeneficiaryAccount = reader.IsDBNull(reader.GetOrdinal("BeneficiaryAccount")) ? null : reader.GetString(reader.GetOrdinal("BeneficiaryAccount")),
                                BeneficiaryBank = reader.IsDBNull(reader.GetOrdinal("BeneficiaryBank")) ? null : reader.GetString(reader.GetOrdinal("BeneficiaryBank")),
                                Type = reader.GetString(reader.GetOrdinal("Type")),
                                TransMethod = reader.GetString(reader.GetOrdinal("TransMethod")),
                                SessionId = reader.GetString(reader.GetOrdinal("SessionId")),
                                CustomerAccountId = reader.GetInt32(reader.GetOrdinal("CustomerAccountId"))


                            };

                            resultData.Add(transaction);
                        }

                        return new ResponseData
                        {
                            Status = HttpStatusCode.OK,
                            ResponseMessage = "Successful",
                            data = resultData
                        };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An error occurred: {ex.Message}");
                    throw;
                }
            }
        }

        public async Task<string> Overdraft()
        {
            var result = _dbContext.customerAccount
                .FirstOrDefault()?.OverDraft.ToString();

            return result ?? "0";
        }

       private bool overDaftAmount(int id)
        {
            return false;
        }
    }
}
