namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("BaoConfig")]
    public partial class BaoConfig
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string BaoName { get; set; }

        [Column(TypeName = "money")]
        public decimal GetCost { get; set; }

        public byte IsDel { get; set; }

        public decimal YearPer { get; set; }
    }
}
