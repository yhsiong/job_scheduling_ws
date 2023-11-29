using Dapper;
using Job_Scheduling.Database;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;

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
        public string BranchCode { get; set; }
        public string DeliverPhone1 { get; set; } 
        public DateTime? CreatedTimeStamp { get; set; }
        public DateTime? LastModified { get; set; }
    

    [NotMapped]
    public class Dto
    {
        public class Get : Quotation
        {
        }
        public class Post : Quotation
        {
        }
        public class Put : Quotation
        {
        }
    }

    [NotMapped]
    public class Operations
    {
        public static async Task<List<Dto.Get>> Read(string connString, string dbName)
        {
            List<Dto.Get> quotations = new List<Dto.Get>();
            using (SqlConnection connectionAutoCount = new SqlConnection(connString))
            {
                connectionAutoCount.Open();
                connectionAutoCount.ChangeDatabase(dbName);
                SqlCommand cmAutoCount = new SqlCommand("select top 500 * from [QT] where Cancelled ='F' and ToDocKey is null order by dockey desc", connectionAutoCount);
                SqlDataReader sdrAutoCount = cmAutoCount.ExecuteReader();

                if (sdrAutoCount.HasRows)
                {
                    while (sdrAutoCount.Read())
                    {
                        Dto.Get quotation = new Dto.Get();
                        var parser = sdrAutoCount.GetRowParser<Dto.Get>(typeof(Dto.Get));
                        quotation = parser(sdrAutoCount);
                        quotations.Add(quotation);
                    }
                } 
                connectionAutoCount.Close();
            }
            return quotations;
        }
        public static async Task<Dto.Get> ReadSingle(string connString, string dbName, string quotation_no)
        {
            Dto.Get quotation = new Dto.Get();
            using (SqlConnection connectionAutoCount = new SqlConnection(connString))
            {
                connectionAutoCount.Open();
                connectionAutoCount.ChangeDatabase(dbName);
                SqlCommand cmAutoCount = new SqlCommand("select * from [QT] where Cancelled ='F' and ToDocKey is null and docNo='" + quotation_no + "' order by dockey desc", connectionAutoCount);
                SqlDataReader sdrAutoCount = cmAutoCount.ExecuteReader();

                if (sdrAutoCount.HasRows)
                {
                    while (sdrAutoCount.Read())
                    { 
                        var parser = sdrAutoCount.GetRowParser<Dto.Get>(typeof(Dto.Get));
                        quotation = parser(sdrAutoCount);
                    }
                }
                connectionAutoCount.Close();
            }
            return quotation;
        }
    }
    }
}