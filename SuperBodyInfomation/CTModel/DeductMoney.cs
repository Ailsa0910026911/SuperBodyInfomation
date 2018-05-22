namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DeductMoney")]
    public partial class DeductMoney
    {
        public int Id { get; set; }

        [Column(TypeName = "money")]
        public decimal Amoney { get; set; }

        public int UId { get; set; }

        public byte TState { get; set; }

        [Required]
        [StringLength(1000)]
        public string Remark { get; set; }

        [Required]
        [StringLength(20)]
        public string CreateAdminName { get; set; }

        public int CreateAdminId { get; set; }

        [StringLength(20)]
        public string AuditAdminName { get; set; }

        public int? AuditAdminId { get; set; }

        public DateTime? AuditTime { get; set; }

        [StringLength(1000)]
        public string AuditRemark { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }

        [Required]
        [StringLength(30)]
        public string UserName { get; set; }
    }
}
