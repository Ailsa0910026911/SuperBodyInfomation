namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("HaoWool")]
    public partial class HaoWool
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(200)]
        public string SmallTitle { get; set; }

        public string Content { get; set; }

        [StringLength(100)]
        public string SmallPic { get; set; }

        [StringLength(100)]
        public string Pic { get; set; }

        public int Click { get; set; }

        public byte IsTop { get; set; }

        public byte State { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }

        [StringLength(200)]
        public string Url { get; set; }
    }
}
