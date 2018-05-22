namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("JobPayWay")]
    public partial class JobPayWay
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        public decimal Cost { get; set; }

        [Column(TypeName = "money")]
        public decimal MinCost { get; set; }

        [Column(TypeName = "money")]
        public decimal MaxCost { get; set; }

        public decimal CostAgent { get; set; }

        [Column(TypeName = "money")]
        public decimal MinAgentCost { get; set; }

        [Column(TypeName = "money")]
        public decimal MaxAgentCost { get; set; }

        [Required]
        [StringLength(100)]
        public string DllName { get; set; }

        [Required]
        [StringLength(20)]
        public string GroupType { get; set; }

        [Required]
        [StringLength(100)]
        public string Version { get; set; }

        public int Sort { get; set; }

        [Required]
        [StringLength(1000)]
        public string QueryArray { get; set; }

        public byte State { get; set; }

        public byte IsDel { get; set; }

        [Column(TypeName = "money")]
        public decimal SNum { get; set; }

        [Column(TypeName = "money")]
        public decimal ENum { get; set; }
    }
}
