namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Orders
    {
        public int Id { get; set; }

        [StringLength(50)]
        public string TNum { get; set; }

        public int UId { get; set; }

        [StringLength(100)]
        public string TName { get; set; }

        public int RUId { get; set; }

        [StringLength(100)]
        public string RName { get; set; }

        public byte TType { get; set; }

        public byte TState { get; set; }

        [Column(TypeName = "money")]
        public decimal Amoney { get; set; }

        public DateTime AddTime { get; set; }

        public byte PayState { get; set; }

        public DateTime? PayTime { get; set; }

        public int PayWay { get; set; }

        public DateTime? TrunTime { get; set; }

        public int Agent { get; set; }

        public byte AgentState { get; set; }

        public DateTime? AgentTime { get; set; }

        [StringLength(1000)]
        public string Remark { get; set; }

        [StringLength(200)]
        public string OrderAddress { get; set; }

        [StringLength(20)]
        public string X { get; set; }

        [StringLength(20)]
        public string Y { get; set; }

        public byte IsDel { get; set; }

        public int AId { get; set; }

        public int FId { get; set; }

        public byte? TrunType { get; set; }

        [StringLength(2000)]
        public string UserCardPic { get; set; }

        [StringLength(50)]
        public string UserCardId { get; set; }

        [StringLength(500)]
        public string BankCardId { get; set; }

        public byte IdCardState { get; set; }

        public DateTime? CardAddTime { get; set; }

        public DateTime? CardEditTime { get; set; }

        public byte PayType { get; set; }

        [StringLength(50)]
        public string PayName { get; set; }

        public byte CardUpType { get; set; }

        [Column(TypeName = "money")]
        public decimal Poundage { get; set; }

        public byte DDAuto { get; set; }

        [StringLength(2000)]
        public string DDUserRm { get; set; }

        public DateTime? DDLastTime { get; set; }

        public byte TDState { get; set; }

        [StringLength(2000)]
        public string InternalRm { get; set; }

        [Column(TypeName = "money")]
        public decimal AgentPayGet { get; set; }

        public byte InState { get; set; }

        public DateTime? InTimed { get; set; }

        public DateTime? InTime { get; set; }

        public byte LagEntryNum { get; set; }

        public byte LagEntryDay { get; set; }

        public byte FrozenState { get; set; }

        public byte RepairState { get; set; }

        public DateTime? CardUpdateTime { get; set; }

        [StringLength(1000)]
        public string DDAuditRemark { get; set; }

        public byte ComeWay { get; set; }

        [StringLength(20)]
        public string UserCardName { get; set; }

        public byte RunSplit { get; set; }
    }
}
