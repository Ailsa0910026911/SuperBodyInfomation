namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("OrderTransfer")]
    public partial class OrderTransfer
    {
        public int Id { get; set; }

        public int UId { get; set; }

        [StringLength(50)]
        public string OId { get; set; }

        public int RUId { get; set; }

        [Column(TypeName = "money")]
        public decimal PayMoney { get; set; }

        [Column(TypeName = "money")]
        public decimal Poundage { get; set; }

        [Column(TypeName = "money")]
        public decimal Amoney { get; set; }

        public double SysRate { get; set; }

        public double UserRate { get; set; }

        public double AgentPayGet { get; set; }

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

        public byte IsMe { get; set; }

        public DateTime? PayTime { get; set; }

        public byte? PayWay { get; set; }

        public byte PayType { get; set; }

        public double AIdPayGet { get; set; }
    }
}
