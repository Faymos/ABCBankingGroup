namespace TransactionService.Model
{
    public class Transactions
    {
        public string AccountNumber { get; set; }
        public double Amount { get; set; }
        public DateTime DateCreated { get; set; }
        public string SenderName { get; set; }
        public string SenderAccount { get; set; }
        public string SenderBank { get; set; }
        public string Remarks { get; set; }
        public string BeneficiaryName { get; set; }
        public string BeneficiaryAccount { get; set; }
        public string BeneficiaryBank { get; set; }
        public string Type { get; set; }
        public string TransMethod { get; set; }
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
}
