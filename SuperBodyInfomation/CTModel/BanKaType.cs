namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("BanKaType")]
    public partial class BanKaType
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(100)]
        public string Pic { get; set; }

        [Column(TypeName = "money")]
        public decimal Amoney { get; set; }

        public byte State { get; set; }

        public DateTime AddTime { get; set; }

        public int Sort { get; set; }

        public byte IsDel { get; set; }
    }
}
