using TransactionService.Model;

namespace TransactionService.Services
{
    public interface IWalletTransaction
    {
        Task<string> GetBalance(string accountNumber);
        Task<ResponseData> GetStatements(string accountNumber, DateTime startdate, DateTime enddate);
        Task<ResponseData> GetStatement(string accountNumber);
        Task<ResponseData> Deposit(string accountNumber, decimal amount);
        Task<ResponseData> Withdrawal(string accountNumber, decimal amount);
        Task<ResponseData> Transfer(string accountNumber, string DestinationAccount, decimal amount);
        Task<ResponseData> TransferToOtherBank(string accountNumber, string DestinationAccount,string bankCode, decimal amount);
        Task<string> AccountVerification(string accountNumber);
        Task<ResponseData> Overdraft(int customerId);
        Task<ResponseData> WithdrawOverDraft(int customerId); 
    }
}
