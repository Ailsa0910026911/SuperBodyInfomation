namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class JobOrders
    {
        public int Id { get; set; }

        public int UId { get; set; }

        [StringLength(50)]
        public string TNum { get; set; }

        [Column(TypeName = "money")]
        public decimal TotalMoney { get; set; }

        [Column(TypeName = "money")]
        public decimal Amount { get; set; }

        [Column(TypeName = "money")]
        public decimal Poundage { get; set; }

        [Column(TypeName = "money")]
        public decimal HFGet { get; set; }

        [Column(TypeName = "money")]
        public decimal AgentGet { get; set; }

        [Column(TypeName = "money")]
        public decimal PayPoundage { get; set; }

        [Column(TypeName = "money")]
        public decimal CashPoundage { get; set; }

        [Column(TypeName = "money")]
        public decimal PayGet { get; set; }

        [Column(TypeName = "money")]
        public decimal CashGet { get; set; }

        public byte State { get; set; }

        public DateTime AddTime { get; set; }

        public byte PayState { get; set; }

        public DateTime? PayTime { get; set; }

        public int PayWay { get; set; }

        public byte PayedState { get; set; }

        public DateTime? PayedTime { get; set; }

        public int CashWay { get; set; }

        public byte IsDel { get; set; }

        public decimal PayRate { get; set; }

        [Column(TypeName = "money")]
        public decimal PayMin { get; set; }

        [Column(TypeName = "money")]
        public decimal PayMax { get; set; }

        public decimal CashRate { get; set; }

        [Column(TypeName = "money")]
        public decimal CashMin { get; set; }

        [Column(TypeName = "money")]
        public decimal CashMax { get; set; }

        public decimal UPayRate { get; set; }

        [Column(TypeName = "money")]
        public decimal UPayMin { get; set; }

        [Column(TypeName = "money")]
        public decimal UPayMax { get; set; }

        public decimal UCashRate { get; set; }

        [Column(TypeName = "money")]
        public decimal UCashMin { get; set; }

        [Column(TypeName = "money")]
        public decimal UCashMax { get; set; }

        public int AgentId { get; set; }

        public byte AgentState { get; set; }

        public DateTime? AgentTime { get; set; }

        public int UserCardId { get; set; }

        public byte AdvCost { get; set; }

        public byte AdvCash { get; set; }

        [StringLength(100)]
        public string Remark { get; set; }

        [StringLength(2000)]
        public string RemarkSplit { get; set; }

        [Column(TypeName = "money")]
        public decimal SameGet { get; set; }
    }
}
