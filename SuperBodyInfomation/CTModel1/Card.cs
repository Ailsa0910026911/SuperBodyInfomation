namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Card")]
    public partial class Card
    {
        public long Id { get; set; }

        [StringLength(50)]
        public string Code { get; set; }

        [StringLength(50)]
        public string PasWd { get; set; }

        public byte? LState { get; set; }

        public int AId { get; set; }

        public byte? State { get; set; }

        public DateTime? AddTime { get; set; }

        public byte IsDel { get; set; }

        public int AdminId { get; set; }

        public int? PUId { get; set; }

        public DateTime? ActTime { get; set; }

        public byte Auto { get; set; }
    }
}
