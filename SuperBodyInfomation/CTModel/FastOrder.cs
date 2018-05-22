namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FastOrder")]
    public partial class FastOrder
    {
        public int Id { get; set; }

        public int UId { get; set; }

        [StringLength(50)]
        public string TNum { get; set; }

        [StringLength(50)]
        public string Trade { get; set; }

        [Required]
        [StringLength(1000)]
        public string PayId { get; set; }

        public byte OType { get; set; }

        [Column(TypeName = "money")]
        public decimal Amoney { get; set; }

        [Column(TypeName = "money")]
        public decimal PayMoney { get; set; }

        [Column(TypeName = "money")]
        public decimal Poundage { get; set; }

        public decimal UserRate { get; set; }

        public decimal AgentRate { get; set; }

        public decimal SysRate { get; set; }

        [Column(TypeName = "money")]
        public decimal SysCash { get; set; }

        [Column(TypeName = "money")]
        public decimal UserCash { get; set; }

        [Column(TypeName = "money")]
        public decimal AgentPayGet { get; set; }

        public int Agent { get; set; }

        [Required]
        [StringLength(500)]
        public string AgentPath { get; set; }

        [StringLength(500)]
        public string Split { get; set; }

        [Column(TypeName = "money")]
        public decimal HFGet { get; set; }

        public byte State { get; set; }

        public DateTime AddTime { get; set; }

        public int? PayWay { get; set; }

        public byte PayState { get; set; }

        public DateTime? PayTime { get; set; }

        public byte AgentState { get; set; }

        public DateTime? AgentTime { get; set; }

        [StringLength(1000)]
        public string Remark { get; set; }

        public byte UserState { get; set; }

        public DateTime? UserTime { get; set; }

        [StringLength(10)]
        public string CardName { get; set; }

        [StringLength(20)]
        public string Bank { get; set; }

        [StringLength(20)]
        public string Card { get; set; }

        [StringLength(20)]
        public string Bin { get; set; }

        public byte IsDel { get; set; }

        public byte ComeWay { get; set; }

        public byte AgentWay { get; set; }

        [Required]
        [StringLength(10)]
        public string CashType { get; set; }

        [Column(TypeName = "money")]
        public decimal SameGet { get; set; }

        [StringLength(50)]
        public string UserId { get; set; }

        [StringLength(10)]
        public string IsProfit { get; set; }

        [StringLength(50)]
        public string OrderType { get; set; }
    }
}
