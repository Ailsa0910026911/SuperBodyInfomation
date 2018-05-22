namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("QRCode")]
    public partial class QRCode
    {
        public int Id { get; set; }

        public int UId { get; set; }

        [Required]
        [StringLength(20)]
        public string Num { get; set; }

        [Required]
        [StringLength(20)]
        public string Code { get; set; }

        [StringLength(500)]
        public string Url { get; set; }

        public DateTime AddTime { get; set; }

        public DateTime? EditTime { get; set; }

        public byte State { get; set; }

        public byte IsDel { get; set; }
    }
}
