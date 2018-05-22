namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("OrderProfitLog")]
    public partial class OrderProfitLog
    {
        public int Id { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }

        public int UId { get; set; }

        [Required]
        [StringLength(50)]
        public string TNum { get; set; }

        public byte LogType { get; set; }

        public byte Tier { get; set; }

        [Column(TypeName = "money")]
        public decimal Profit { get; set; }

        [StringLength(50)]
        public string UserName { get; set; }

        public byte OrderType { get; set; }

        [Column(TypeName = "money")]
        public decimal Amoney { get; set; }

        public int Agent { get; set; }
    }
}
