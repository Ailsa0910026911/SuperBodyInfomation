namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("APPModule")]
    public partial class APPModule
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(500)]
        public string PicUrl { get; set; }

        [StringLength(500)]
        public string PictureUrl { get; set; }

        [StringLength(500)]
        public string Value { get; set; }

        public byte ModuleType { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }

        public byte State { get; set; }

        public byte DisplaySite { get; set; }

        public int Sort { get; set; }

        public int AgentId { get; set; }

        public bool IsLock { get; set; }

        public int Version { get; set; }

        public int Height { get; set; }

        public int Width { get; set; }
    }
}
