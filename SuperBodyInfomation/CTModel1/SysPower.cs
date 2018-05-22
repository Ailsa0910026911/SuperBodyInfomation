namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SysPower")]
    public partial class SysPower
    {
        public int Id { get; set; }

        public int PId { get; set; }

        [StringLength(30)]
        public string Name { get; set; }

        [StringLength(50)]
        public string Ctrl { get; set; }

        [StringLength(50)]
        public string Method { get; set; }

        public byte State { get; set; }

        [StringLength(200)]
        public string Remark { get; set; }

        public byte IsDel { get; set; }

        public byte PType { get; set; }

        public int Sort { get; set; }
    }
}
