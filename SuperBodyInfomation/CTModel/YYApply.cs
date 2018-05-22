namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("YYApply")]
    public partial class YYApply
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string ApplyId { get; set; }

        public int Num { get; set; }

        [Required]
        [StringLength(200)]
        public string Reason { get; set; }

        public int? Poi_Id { get; set; }

        public DateTime AddTime { get; set; }

        public byte State { get; set; }

        public DateTime? EditTime { get; set; }

        public byte IsDel { get; set; }
    }
}
