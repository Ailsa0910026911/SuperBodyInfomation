namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("BaoLog")]
    public partial class BaoLog
    {
        public int Id { get; set; }

        public int UId { get; set; }

        public short LType { get; set; }

        [Column(TypeName = "money")]
        public decimal BeforAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal BeforFrozen { get; set; }

        [Column(TypeName = "money")]
        public decimal Amount { get; set; }

        [Column(TypeName = "money")]
        public decimal AfterAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal AfterFrozen { get; set; }

        public byte State { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }
    }
}
