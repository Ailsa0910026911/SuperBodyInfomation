namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SalesLog")]
    public partial class SalesLog
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Salesman { get; set; }

        public DateTime ExpireTime { get; set; }

        [Column(TypeName = "money")]
        public decimal AgentFee { get; set; }

        public int AgentId { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }

        public byte State { get; set; }
    }
}
