namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UsersMoveLog")]
    public partial class UsersMoveLog
    {
        public int Id { get; set; }

        public int FromSAId { get; set; }

        [Required]
        [StringLength(100)]
        public string FromName { get; set; }

        public int ToSAId { get; set; }

        [Required]
        [StringLength(100)]
        public string ToName { get; set; }

        public int UId { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }

        [Required]
        [StringLength(20)]
        public string UTrueName { get; set; }

        [StringLength(100)]
        public string OpName { get; set; }

        public byte Type { get; set; }

        [StringLength(50)]
        public string Remark { get; set; }

        [StringLength(50)]
        public string Tel { get; set; }
    }
}
