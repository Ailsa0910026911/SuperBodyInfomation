namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ShareTotal")]
    public partial class ShareTotal
    {
        public int Id { get; set; }

        public int UId { get; set; }

        public int ShareNum { get; set; }

        [Column(TypeName = "money")]
        public decimal Amount { get; set; }

        [Column(TypeName = "money")]
        public decimal Profit { get; set; }

        public byte Tier { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }
    }
}
