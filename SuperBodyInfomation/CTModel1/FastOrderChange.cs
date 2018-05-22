namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FastOrderChange")]
    public partial class FastOrderChange
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string TNum { get; set; }

        [Required]
        [StringLength(50)]
        public string STNum { get; set; }

        public byte State { get; set; }

        [Required]
        public string Data { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }

        [Required]
        [StringLength(50)]
        public string AdminName { get; set; }

        public int AdminId { get; set; }
    }
}
