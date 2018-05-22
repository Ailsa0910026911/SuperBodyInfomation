namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("CheckUser")]
    public partial class CheckUser
    {
        public int Id { get; set; }

        [Column(TypeName = "money")]
        public decimal PastMoney { get; set; }

        [Column(TypeName = "money")]
        public decimal PastFrozen { get; set; }

        [Column(TypeName = "money")]
        public decimal CheckMoney { get; set; }

        [Column(TypeName = "money")]
        public decimal CheckFrozen { get; set; }

        public DateTime AddTime { get; set; }

        [Column(TypeName = "money")]
        public decimal NowMoney { get; set; }

        [Column(TypeName = "money")]
        public decimal NowFrozen { get; set; }

        public byte IsDel { get; set; }

        public DateTime TaskDate { get; set; }

        public int UId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Remark { get; set; }
    }
}
