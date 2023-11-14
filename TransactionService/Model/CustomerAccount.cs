namespace TransactionService.Model
{
    public class CustomerAccount
    {
        public int Id { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public double TotalBalance { get; set; }
        public double? CrAmount { get; set; }
        public double? DrAmount { get; set; }
        public double? InterestAmount { get; set; }
        public double CurrentBalance { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public int CustomerId { get; set; }
        
    }
}
