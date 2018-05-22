namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FastSplit")]
    public partial class FastSplit
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Tnum { get; set; }

        [Column(TypeName = "money")]
        public decimal Profit { get; set; }

        public int AgentId { get; set; }

        public byte Tier { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }
    }
}
