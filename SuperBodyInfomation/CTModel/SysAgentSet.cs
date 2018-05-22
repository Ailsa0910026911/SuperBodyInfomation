namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SysAgentSet")]
    public partial class SysAgentSet
    {
        public int Id { get; set; }

        public int AId { get; set; }

        [StringLength(10)]
        public string Tag { get; set; }

        public byte SV { get; set; }

        public byte IsDel { get; set; }
    }
}
