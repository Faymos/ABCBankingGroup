using TransactionService.Model;

namespace TransactionService.Services
{
    public interface IWalletTransaction
    {
        Task<string> GetBalance(string accountNumber);
        Task<ResponseData> GetStatements(string accountNumber, DateTime startdate, DateTime enddate);
        Task<ResponseData> Deposit(string accountNumber, double amount);
        Task<ResponseData> Withdrawal(string accountNumber, double amount);
        Task<ResponseData> Transfer(string accountNumber, string DestinationAccount, double amount);
        Task<ResponseData> TransferToOtherBank(string accountNumber, string DestinationAccount,string bankCode, double amount);
    }
}
