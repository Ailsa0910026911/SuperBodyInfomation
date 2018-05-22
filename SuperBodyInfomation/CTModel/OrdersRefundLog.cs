namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("OrdersRefundLog")]
    public partial class OrdersRefundLog
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string TNum { get; set; }

        public byte LogType { get; set; }

        [Required]
        [StringLength(2000)]
        public string Remark { get; set; }

        [Required]
        [StringLength(50)]
        public string Img { get; set; }

        [Required]
        [StringLength(50)]
        public string AdminName { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }

        public int AdminId { get; set; }
    }
}
