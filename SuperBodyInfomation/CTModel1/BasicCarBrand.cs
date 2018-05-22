namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("BasicCarBrand")]
    public partial class BasicCarBrand
    {
        public int Id { get; set; }

        [StringLength(20)]
        public string Name { get; set; }

        public double? Score { get; set; }

        [StringLength(200)]
        public string Logo { get; set; }

        [StringLength(10)]
        public string Letter { get; set; }

        public byte IsDel { get; set; }
    }
}
