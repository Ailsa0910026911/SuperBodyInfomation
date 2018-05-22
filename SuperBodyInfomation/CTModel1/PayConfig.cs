namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PayConfig")]
    public partial class PayConfig
    {
        public int Id { get; set; }

        [StringLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }

        public double? Cost { get; set; }

        public double? CostUser { get; set; }

        [StringLength(100)]
        public string DllName { get; set; }

        [StringLength(100)]
        public string Version { get; set; }

        public int? Sort { get; set; }

        [StringLength(1000)]
        public string QueryArray { get; set; }

        public byte State { get; set; }

        public byte IsDel { get; set; }

        public double CostAgent { get; set; }

        [StringLength(20)]
        public string GroupType { get; set; }

        public byte IsPay { get; set; }
    }
}
