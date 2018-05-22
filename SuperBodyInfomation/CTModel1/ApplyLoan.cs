namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ApplyLoan")]
    public partial class ApplyLoan
    {
        public int Id { get; set; }

        public int? UId { get; set; }

        [StringLength(10)]
        public string TrueName { get; set; }

        [StringLength(10)]
        public string Sex { get; set; }

        [StringLength(20)]
        public string IDcard { get; set; }

        [StringLength(20)]
        public string Mobile { get; set; }

        [StringLength(20)]
        public string Company { get; set; }

        public int? ComProvince { get; set; }

        public int? ComCity { get; set; }

        public int? ComDistrict { get; set; }

        [StringLength(50)]
        public string ComAddress { get; set; }

        [StringLength(20)]
        public string ComTel { get; set; }

        [StringLength(20)]
        public string Position { get; set; }

        [StringLength(20)]
        public string Education { get; set; }

        public byte HasSheBao { get; set; }

        [StringLength(50)]
        public string SheBaoTime { get; set; }

        public byte Marry { get; set; }

        public byte HasCar { get; set; }

        public int? CarBrand { get; set; }

        [StringLength(50)]
        public string House { get; set; }

        public double? Income { get; set; }

        [StringLength(10)]
        public string Payment { get; set; }

        public byte HasCredit { get; set; }

        public int? CreditBankId { get; set; }

        public double Amount { get; set; }

        public int? MyScore { get; set; }

        public byte State { get; set; }

        public int AId { get; set; }

        public byte IsDel { get; set; }

        public DateTime AddTime { get; set; }

        public int AgentId { get; set; }

        public int AgentAId { get; set; }

        [Column(TypeName = "money")]
        public decimal Amoney { get; set; }

        public byte PayState { get; set; }

        public DateTime? PayTime { get; set; }

        public byte AgentPay { get; set; }

        [Column(TypeName = "money")]
        public decimal AgentMoney { get; set; }

        [Column(TypeName = "money")]
        public decimal AIdMoney { get; set; }

        [Required]
        [StringLength(20)]
        public string SheBao { get; set; }

        [Required]
        [StringLength(20)]
        public string CompanyNature { get; set; }
    }
}
