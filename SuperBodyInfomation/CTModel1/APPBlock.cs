namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("APPBlock")]
    public partial class APPBlock
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Name { get; set; }

        [StringLength(30)]
        public string SubName { get; set; }

        [Required]
        [StringLength(500)]
        public string IconUrl { get; set; }

        [Required]
        [StringLength(500)]
        public string PicUrl { get; set; }

        [StringLength(20)]
        public string LinkName1 { get; set; }

        [StringLength(500)]
        public string LinkUrl1 { get; set; }

        public byte? LinkType1 { get; set; }

        [StringLength(20)]
        public string LinkName2 { get; set; }

        [StringLength(500)]
        public string LinkUrl2 { get; set; }

        public byte? LinkType2 { get; set; }

        public int AgentId { get; set; }

        public int Sort { get; set; }

        public byte State { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }

        [StringLength(500)]
        public string LinkUrl { get; set; }

        public byte? LinkType { get; set; }

        public int Height { get; set; }

        public int Width { get; set; }
    }
}
