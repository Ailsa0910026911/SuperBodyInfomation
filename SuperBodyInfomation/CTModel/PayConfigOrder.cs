namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PayConfigOrder")]
    public partial class PayConfigOrder
    {
        public int Id { get; set; }

        public int PCCId { get; set; }

        public int UId { get; set; }

        [StringLength(50)]
        public string OId { get; set; }

        [Column(TypeName = "money")]
        public decimal Amoney { get; set; }

        public int Agent { get; set; }

        public int AId { get; set; }

        public byte OrderState { get; set; }

        public byte PayState { get; set; }

        public DateTime AddTime { get; set; }

        [StringLength(1000)]
        public string Remark { get; set; }

        public byte IsDel { get; set; }

        public DateTime? PayTime { get; set; }

        public byte? PayWay { get; set; }

        public double AgentGet { get; set; }

        public byte AgentState { get; set; }

        public double AIdGet { get; set; }

        public double SysRate { get; set; }

        [Column(TypeName = "money")]
        public decimal Poundage { get; set; }
    }
}
