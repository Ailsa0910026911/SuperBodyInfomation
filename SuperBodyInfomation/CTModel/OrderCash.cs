namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("OrderCash")]
    public partial class OrderCash
    {
        public int Id { get; set; }

        public int UId { get; set; }

        [StringLength(50)]
        public string OId { get; set; }

        [Required]
        [StringLength(20)]
        public string Owner { get; set; }

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
        [StringLength(50)]
        public string Deposit { get; set; }

        [StringLength(20)]
        public string Mobile { get; set; }

        [Column(TypeName = "money")]
        public decimal Amoney { get; set; }

        public double UserRate { get; set; }

        [Column(TypeName = "money")]
        public decimal CashRate { get; set; }

        public double AgentCashGet { get; set; }

        [StringLength(1000)]
        public string Remark { get; set; }

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

        public byte? TrunType { get; set; }

        [Column(TypeName = "money")]
        public decimal Cash { get; set; }

        [Column(TypeName = "money")]
        public decimal ECash { get; set; }

        public double AIdCashGet { get; set; }

        public DateTime? AuditTime { get; set; }

        public DateTime? RefundTime { get; set; }
    }
}
