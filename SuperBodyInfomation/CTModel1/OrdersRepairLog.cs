namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("OrdersRepairLog")]
    public partial class OrdersRepairLog
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string TNum { get; set; }

        public byte LogType { get; set; }

        [StringLength(2000)]
        public string Remark { get; set; }

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
