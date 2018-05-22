namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SysMoneySet")]
    public partial class SysMoneySet
    {
        public int Id { get; set; }

        public byte IsDel { get; set; }

        public decimal PaySplitA1 { get; set; }

        public decimal PaySplitA2 { get; set; }

        public decimal PaySplitA3 { get; set; }

        public decimal PaySplitA4 { get; set; }

        public decimal PaySplitA5 { get; set; }

        public decimal PaySplitA6 { get; set; }

        public decimal PaySplitU0 { get; set; }

        public decimal PaySplitU1 { get; set; }

        public decimal PaySplitU2 { get; set; }

        [Column(TypeName = "money")]
        public decimal VipPrice { get; set; }

        public decimal VipSplitA1 { get; set; }

        public decimal VipSplitA2 { get; set; }

        public decimal VipSplitA3 { get; set; }

        public decimal VipSplitA4 { get; set; }

        public decimal VipSplitA5 { get; set; }

        public decimal VipSplitA6 { get; set; }

        public decimal VipSplitU0 { get; set; }

        public decimal VipSplitU1 { get; set; }

        public decimal VipSplitU2 { get; set; }

        [Column(TypeName = "money")]
        public decimal AgentPrice1 { get; set; }

        [Column(TypeName = "money")]
        public decimal AgentPrice2 { get; set; }

        [Column(TypeName = "money")]
        public decimal AgentPrice3 { get; set; }

        [Column(TypeName = "money")]
        public decimal AgentPrice4 { get; set; }

        [Column(TypeName = "money")]
        public decimal AgentPrice5 { get; set; }

        [Column(TypeName = "money")]
        public decimal AgentPrice6 { get; set; }

        public decimal AgentSplit0 { get; set; }

        public decimal AgentSplit1 { get; set; }

        public decimal AgentSplit2 { get; set; }

        public decimal AgentSplit3 { get; set; }

        public decimal AgentSplit4 { get; set; }

        public decimal AgentSplit5 { get; set; }

        public decimal SameAgent { get; set; }

        public decimal JobSplitA1 { get; set; }

        public decimal JobSplitA2 { get; set; }

        public decimal JobSplitA3 { get; set; }

        public decimal JobSplitA4 { get; set; }

        public decimal JobSplitA5 { get; set; }

        public decimal JobSplitA6 { get; set; }

        public decimal JobSplitU0 { get; set; }

        public decimal JobSplitU1 { get; set; }

        public decimal JobSplitU2 { get; set; }
    }
}
