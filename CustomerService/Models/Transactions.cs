﻿namespace CustomerService.Models
{
    public class Transactions
    {
        public string AccountNumber { get; set; }
        public float Amount { get; set; }
        public DateTime DateCreated { get; set; }
        public string SenderName { get; set; }
        public string SenderAccount { get; set; }
        public string SenderBank { get; set; }
        public string Remarks { get; set; }
        public string BeneficiaryName { get; set; }
        public string BeneficiaryAccount { get; set; }
        public string BeneficiaryBank { get; set; }
        public TransType Type { get; set; }
        public TransMethod TransMethod { get; set; }
        public string CustomerAccountId { get; set; }
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