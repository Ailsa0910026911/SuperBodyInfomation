namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("YYPage")]
    public partial class YYPage
    {
        public int Id { get; set; }

        [StringLength(20)]
        public string PageId { get; set; }

        public int UId { get; set; }

        [Required]
        [StringLength(6)]
        public string Title { get; set; }

        [Required]
        [StringLength(7)]
        public string SubTitle { get; set; }

        [Required]
        [StringLength(200)]
        public string Url { get; set; }

        [Required]
        [StringLength(200)]
        public string WXIcon { get; set; }

        [Required]
        [StringLength(200)]
        public string Icon { get; set; }

        [StringLength(200)]
        public string Comment { get; set; }

        public DateTime AddTime { get; set; }

        public byte State { get; set; }

        public byte IsDel { get; set; }
    }
}
