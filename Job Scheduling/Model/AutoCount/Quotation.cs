namespace Job_Scheduling.Model.AutoCount
{
    public class Quotation
    {
        public int? DocKey { get; set; }
        public string DocNo { get; set; } 
        public string DebtorCode { get; set; }
        public string DebtorName { get; set; }
        public string Description { get; set; } 
        public string SalesAgent { get; set; }
        public string Validity { get; set; } 
        public string CurrencyCode { get; set; }
        public decimal TotalExTax { get; set; }
        public decimal TaxableAmt { get; set; }
        public decimal FinalTotal { get; set; }
        public string ToDocType { get; set; }
        public string ToDocKey { get; set; }
        public string Cancelled { get; set; }
        public string CreatedUserID { get; set; }
        public string LastModifiedUserID { get; set; } 
        public DateTime? CreatedTimeStamp { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
