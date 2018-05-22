namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("OrdersDDLog")]
    public partial class OrdersDDLog
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string TNum { get; set; }

        public byte LogType { get; set; }

        [StringLength(2000)]
        public string Remark { get; set; }

        [StringLength(2000)]
        public string Img { get; set; }

        public DateTime? LastTime { get; set; }

        [Required]
        [StringLength(50)]
        public string OpName { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }

        [StringLength(2000)]
        public string InteriorRemark { get; set; }
    }
}
