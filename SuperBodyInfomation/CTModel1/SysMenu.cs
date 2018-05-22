namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SysMenu")]
    public partial class SysMenu
    {
        public int Id { get; set; }

        public int PId { get; set; }

        [StringLength(20)]
        public string Name { get; set; }

        [StringLength(200)]
        public string Url { get; set; }

        [StringLength(200)]
        public string Remark { get; set; }

        public int Sort { get; set; }

        public byte IsDel { get; set; }

        [StringLength(20)]
        public string Ico { get; set; }

        public byte MType { get; set; }
    }
}
