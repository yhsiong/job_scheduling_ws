using Dapper;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;

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


        [NotMapped]
        public class Dto
        {
            public class Get : Quotation_Detail
            {
            }
            public class Post : Quotation_Detail
            {
            }
            public class Put : Quotation_Detail
            {
            }
        }

        [NotMapped]
        public class Operations
        {
            public static async Task<List<Dto.Get>> Read(string connString, string dbName, string quotation_no)
            {
                List<Dto.Get> quotationDetails = new List<Dto.Get>();
                using (SqlConnection connectionAutoCount = new SqlConnection(connString))
                {
                    connectionAutoCount.Open();
                    connectionAutoCount.ChangeDatabase(dbName);
                    SqlCommand cmAutoCount = new SqlCommand("select * from [QT] as a inner join [QTDTL] as b on a.DocKey=b.DocKey  where a.DocNo='" + quotation_no + "' order by seq asc", connectionAutoCount);
                    SqlDataReader sdrAutoCount = cmAutoCount.ExecuteReader();

                    if (sdrAutoCount.HasRows)
                    {
                        while (sdrAutoCount.Read())
                        {
                            Dto.Get quotationDetail = new Dto.Get();
                            var parser = sdrAutoCount.GetRowParser<Dto.Get>(typeof(Dto.Get));
                            quotationDetail = parser(sdrAutoCount);
                            quotationDetails.Add(quotationDetail);
                        }
                    }
                    connectionAutoCount.Close();
                }
                return quotationDetails;
            }
        }
        }
}
