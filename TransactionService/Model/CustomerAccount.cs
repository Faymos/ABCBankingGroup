namespace TransactionService.Model
{
    public class CustomerAccount
    {
        public int Id { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal? CrAmount { get; set; }
        public decimal? DrAmount { get; set; }
        public decimal? OverDraft { get; set; }
        public decimal CurrentBalance { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public int CustomerId { get; set; }
        public bool IsActive { get; set; }
        public bool IsOverdraft {  get; set; }
    }
}
