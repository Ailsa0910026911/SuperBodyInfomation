namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TurnProc")]
    public partial class TurnProc
    {
        public int Id { get; set; }

        public int TId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Column(TypeName = "money")]
        public decimal Amoney { get; set; }

        public int Num { get; set; }

        public int UNum { get; set; }

        public byte IsDel { get; set; }
    }
}
