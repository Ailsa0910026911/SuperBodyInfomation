namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TurnLog")]
    public partial class TurnLog
    {
        public int Id { get; set; }

        public int UId { get; set; }

        public int TId { get; set; }

        public int TPId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Column(TypeName = "money")]
        public decimal Amoney { get; set; }

        public int Num { get; set; }

        public byte State { get; set; }

        public DateTime AddTime { get; set; }

        public DateTime? TakeTime { get; set; }

        public byte IsDel { get; set; }
    }
}
