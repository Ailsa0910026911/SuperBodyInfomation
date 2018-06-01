namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("BusinessInfo")]
    public partial class BusinessInfo
    {
        public int Id { get; set; }

        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Url { get; set; }

        public int? Type { get; set; }

        [StringLength(100)]
        public string Img { get; set; }

        [StringLength(50)]
        public string OneDescribe { get; set; }

        [StringLength(500)]
        public string Describe { get; set; }

        [StringLength(50)]
        public string Limit { get; set; }

        [StringLength(50)]
        public string SuccessRate { get; set; }

        [StringLength(50)]
        public string CompanyName { get; set; }

        [StringLength(50)]
        public string InterestRate { get; set; }

        [StringLength(50)]
        public string ServiceCharge { get; set; }

        [StringLength(500)]
        public string Age { get; set; }

        [StringLength(50)]
        public string ApplicationMaterial { get; set; }

        [StringLength(50)]
        public string HandlingTime { get; set; }

        [StringLength(50)]
        public string LengthOfMaturity { get; set; }

        [StringLength(50)]
        public string ModeOfRepayment { get; set; }

        [StringLength(50)]
        public string Prepayment { get; set; }

        [StringLength(50)]
        public string ReviewTheWay { get; set; }

        [StringLength(50)]
        public string CallBack { get; set; }

        [StringLength(50)]
        public string MoneyWay { get; set; }

        [StringLength(50)]
        public string MoneyDelete { get; set; }

        [StringLength(50)]
        public string SelectCredit { get; set; }

        [StringLength(50)]
        public string DownloadAPP { get; set; }

        [StringLength(500)]
        public string ApplicationProcedure { get; set; }

        [StringLength(50)]
        public string Adress { get; set; }
        [StringLength(50)]
        public string Phone { get; set; }
        [StringLength(50)]
        public string AuthorizatonReason { get; set; }
        [StringLength(50)]
        public string Edit { get; set; }
    }
}
