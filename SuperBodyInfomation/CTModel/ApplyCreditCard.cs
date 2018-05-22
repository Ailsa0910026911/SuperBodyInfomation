namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ApplyCreditCard")]
    public partial class ApplyCreditCard
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderNum { get; set; }

        public int Uid { get; set; }

        [Required]
        [StringLength(100)]
        public string UserName { get; set; }

        [Required]
        [StringLength(50)]
        public string UserMobile { get; set; }

        [Required]
        [StringLength(50)]
        public string UserIdCard { get; set; }

        public int BankId { get; set; }

        public int FirstAgentId { get; set; }

        public int AgentId { get; set; }

        [StringLength(50)]
        public string Relation { get; set; }

        public int SettlementState { get; set; }

        public decimal? SettlementAmount { get; set; }

        public decimal? FirstAgentAmount { get; set; }

        public int FirstAgentAmountState { get; set; }

        public int State { get; set; }

        public DateTime AddTime { get; set; }

        public DateTime? SettlementTime { get; set; }

        public DateTime? FirstAgentTime { get; set; }

        public byte IsDel { get; set; }
    }
}
