namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("OrderHouse")]
    public partial class OrderHouse
    {
        public int Id { get; set; }

        public int UId { get; set; }

        [StringLength(50)]
        public string OId { get; set; }

        [Required]
        [StringLength(20)]
        public string HouseOwner { get; set; }

        [Required]
        [StringLength(20)]
        public string Bank { get; set; }

        [Required]
        [StringLength(20)]
        public string CardNum { get; set; }

        public int? Province { get; set; }

        public int? City { get; set; }

        public int? District { get; set; }

        [StringLength(20)]
        public string Bin { get; set; }

        [Required]
        [StringLength(20)]
        public string Deposit { get; set; }

        [StringLength(20)]
        public string Mobile { get; set; }

        [Column(TypeName = "money")]
        public decimal MonthRent { get; set; }

        [Column(TypeName = "money")]
        public decimal SecurityMoney { get; set; }

        public int PayMonth { get; set; }

        [Column(TypeName = "money")]
        public decimal PayMoney { get; set; }

        [Column(TypeName = "money")]
        public decimal Poundage { get; set; }

        [Column(TypeName = "money")]
        public decimal Amoney { get; set; }

        public double SysRate { get; set; }

        public double UserRate { get; set; }

        [Column(TypeName = "money")]
        public decimal CashRate { get; set; }

        public double AgentPayGet { get; set; }

        public byte IsDel { get; set; }

        public int Agent { get; set; }

        public int AId { get; set; }

        public int FId { get; set; }

        public byte OrderState { get; set; }

        public byte PayState { get; set; }

        public byte AgentState { get; set; }

        public DateTime AddTime { get; set; }

        public byte FState { get; set; }

        public DateTime? FTime { get; set; }

        public DateTime? StartTime { get; set; }

        public byte? TrunType { get; set; }

        [StringLength(500)]
        public string Remark { get; set; }

        public DateTime? PayTime { get; set; }

        public byte? PayWay { get; set; }

        public double AIdPayGet { get; set; }
    }
}
