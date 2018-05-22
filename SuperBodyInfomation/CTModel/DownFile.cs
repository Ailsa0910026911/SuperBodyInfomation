namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DownFile")]
    public partial class DownFile
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Pic { get; set; }

        public int TId { get; set; }

        public byte? State { get; set; }

        public int Sort { get; set; }

        public byte IsDel { get; set; }

        public DateTime AddTime { get; set; }
    }
}
