namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("AdTag")]
    public partial class AdTag
    {
        public int Id { get; set; }

        [StringLength(10)]
        public string Tag { get; set; }

        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(200)]
        public string Info { get; set; }

        public byte State { get; set; }

        public int Sort { get; set; }

        public byte IsDel { get; set; }
    }
}
