namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("YYDaily")]
    public partial class YYDaily
    {
        public int Id { get; set; }

        public int UId { get; set; }

        [Required]
        [StringLength(20)]
        public string DevId { get; set; }

        public DateTime OutDate { get; set; }

        [StringLength(20)]
        public string PageId { get; set; }

        public DateTime AddTime { get; set; }

        public byte State { get; set; }

        public byte IsDel { get; set; }

        public int ClickPV { get; set; }

        public int ClickUV { get; set; }

        public int ShakePV { get; set; }

        public int ShakeUV { get; set; }
    }
}
