namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("OrdersPayOnly")]
    public partial class OrdersPayOnly
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string TNum { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }
    }
}
