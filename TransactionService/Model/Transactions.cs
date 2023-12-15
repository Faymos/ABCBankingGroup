namespace TransactionService.Model
{
    public class Transactions
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; }
        public decimal Amount { get; set; }
        public DateTime DateCreated { get; set; }
        public string? SenderName { get; set; }
        public string? SenderAccount { get; set; }
        public string? SenderBank { get; set; }
        public string? Remarks { get; set; }
        public string? BeneficiaryName { get; set; }
        public string? BeneficiaryAccount { get; set; }
        public string? BeneficiaryBank { get; set; }
        public string Type { get; set; }
        public string TransMethod { get; set; }
        public string SessionId { get; set; }
        public int CustomerAccountId { get; set; }
    }
    public enum TransType
    {
        cr,
        dr
    }

    public enum TransMethod
    {
        transfer,
        withdrawal,
        deposit
    }

    public class Summary
    {
        public string? TotalUser { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal TotalOverDraft { get; set; }
    }
}
