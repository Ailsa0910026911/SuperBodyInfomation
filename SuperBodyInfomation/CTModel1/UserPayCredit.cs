namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UserPayCredit")]
    public partial class UserPayCredit
    {
        public int Id { get; set; }

        public int UId { get; set; }

        [Column(TypeName = "money")]
        public decimal Amount { get; set; }

        [StringLength(50)]
        public string TrueName { get; set; }

        [StringLength(50)]
        public string Mobile { get; set; }

        public string Remark { get; set; }

        public byte State { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }

        public int AgentId { get; set; }

        public int AId { get; set; }
    }
}
