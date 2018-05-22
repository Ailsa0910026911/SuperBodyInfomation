namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FastConfig")]
    public partial class FastConfig
    {
        public int Id { get; set; }

        public decimal UserCost { get; set; }

        [Column(TypeName = "money")]
        public decimal UserCash { get; set; }

        public byte IsDel { get; set; }

        public byte AgentWay { get; set; }
    }
}
