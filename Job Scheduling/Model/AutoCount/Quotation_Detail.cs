namespace Job_Scheduling.Model.AutoCount
{
    public class Quotation_Detail
    {
        public int? DtlKey { get; set; }
        public int? DocKey { get; set; }
        public int? Seq { get; set; }

        public string MainItem { get; set; }
        public string ItemCode { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public string FurtherDescription { get; set; }

        public float? Qty { get; set; }
        public float? Rate { get; set; }
        public float? UnitPrice { get; set; }
        public float? DiscountAmt { get; set; }
        public float? Tax { get; set; }
        public float? SubTotal { get; set; }
        public float? SubTotalExTax { get; set; }
        public float? LocalTax { get; set; }
        public float? TaxableAmt { get; set; }
        public float? LocalSubTotalExTax { get; set; }
        public float? TaxRate { get; set; }
          
    }
}
