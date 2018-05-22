namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("CheckResult")]
    public partial class CheckResult
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string TNum { get; set; }

        public int CheckType { get; set; }

        [Required]
        [StringLength(100)]
        public string CheckMsg { get; set; }

        public DateTime CheckTime { get; set; }

        [Column(TypeName = "money")]
        public decimal Amount { get; set; }

        public byte IsDel { get; set; }

        public DateTime TaskDate { get; set; }

        public int UId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Remark { get; set; }
    }
}
