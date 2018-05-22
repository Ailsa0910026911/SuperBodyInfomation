namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("OrdersRec")]
    public partial class OrdersRec
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

        public byte TState { get; set; }

        [Column(TypeName = "money")]
        public decimal Amoney { get; set; }

        public DateTime AddTime { get; set; }

        public DateTime? PayTime { get; set; }

        public int PayWay { get; set; }

        [StringLength(1000)]
        public string Remark { get; set; }

        [StringLength(200)]
        public string OrderAddress { get; set; }

        [StringLength(20)]
        public string X { get; set; }

        [StringLength(20)]
        public string Y { get; set; }

        [Column(TypeName = "money")]
        public decimal Poundage { get; set; }

        public byte InState { get; set; }

        public byte IsDel { get; set; }
    }
}
