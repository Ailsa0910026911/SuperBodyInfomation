namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("BanKaList")]
    public partial class BanKaList
    {
        public int Id { get; set; }

        public int BKTId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(100)]
        public string Pic { get; set; }

        [StringLength(500)]
        public string Url { get; set; }

        public byte State { get; set; }

        public DateTime AddTime { get; set; }

        public int Sort { get; set; }

        public byte IsDel { get; set; }

        public int Click { get; set; }
    }
}
