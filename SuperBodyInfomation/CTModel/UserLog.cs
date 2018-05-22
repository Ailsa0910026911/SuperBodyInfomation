namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UserLog")]
    public partial class UserLog
    {
        public int Id { get; set; }

        public int UId { get; set; }

        [StringLength(100)]
        public string OId { get; set; }

        public short OType { get; set; }

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

        [StringLength(2000)]
        public string Remark { get; set; }
    }
}
