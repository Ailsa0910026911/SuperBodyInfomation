namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UserPromoteGet")]
    public partial class UserPromoteGet
    {
        public int Id { get; set; }

        public int AgentID { get; set; }

        public byte PromoteLevel { get; set; }

        public decimal PromoteGet { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }

        public byte State { get; set; }
    }
}
