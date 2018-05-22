namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("CutAct")]
    public partial class CutAct
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(2000)]
        public string PInfo { get; set; }

        public DateTime STime { get; set; }

        public DateTime ETime { get; set; }

        public int OutTime { get; set; }

        public int Amount { get; set; }

        public DateTime AddTime { get; set; }

        public byte State { get; set; }

        public byte IsDel { get; set; }
    }
}
