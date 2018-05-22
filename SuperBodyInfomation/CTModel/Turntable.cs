namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Turntable")]
    public partial class Turntable
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(2000)]
        public string PInfo { get; set; }

        [StringLength(2000)]
        public string PTips { get; set; }

        public DateTime STime { get; set; }

        public DateTime ETime { get; set; }

        [StringLength(200)]
        public string ETips { get; set; }

        [StringLength(200)]
        public string ETitle { get; set; }

        [StringLength(2000)]
        public string EInfo { get; set; }

        public int BaseNum { get; set; }

        public DateTime AddTime { get; set; }

        public byte State { get; set; }

        public byte IsDel { get; set; }
    }
}
