namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("VIPOrder")]
    public partial class VIPOrder
    {
        public int Id { get; set; }

        [StringLength(100)]
        public string VName { get; set; }

        public int UId { get; set; }

        [StringLength(50)]
        public string TNum { get; set; }

        [Column(TypeName = "money")]
        public decimal Amoney { get; set; }

        [Column(TypeName = "money")]
        public decimal SplitMoney { get; set; }

        public byte SplitState { get; set; }

        [Column(TypeName = "money")]
        public decimal Poundage { get; set; }

        [Column(TypeName = "money")]
        public decimal HFGet { get; set; }

        public int PayWay { get; set; }

        public byte State { get; set; }

        public byte PayState { get; set; }

        public DateTime? PayTime { get; set; }

        public DateTime AddTime { get; set; }

        [StringLength(1000)]
        public string Remark { get; set; }

        public int Days { get; set; }

        public byte UserState { get; set; }

        public DateTime? UserTime { get; set; }

        public int Agent { get; set; }

        public byte IsDel { get; set; }

        [Column(TypeName = "money")]
        public decimal SameGet { get; set; }
    }
}
