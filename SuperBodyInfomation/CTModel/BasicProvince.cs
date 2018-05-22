namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("BasicProvince")]
    public partial class BasicProvince
    {
        public int Id { get; set; }

        [StringLength(100)]
        public string Name { get; set; }

        public byte State { get; set; }

        public byte IsDel { get; set; }
    }
}
