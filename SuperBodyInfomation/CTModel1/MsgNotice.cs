namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("MsgNotice")]
    public partial class MsgNotice
    {
        public int Id { get; set; }

        public byte NType { get; set; }

        [StringLength(100)]
        public string Name { get; set; }

        public string Info { get; set; }

        public byte Level { get; set; }

        public byte State { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }

        public string ReadUsers { get; set; }

        public string ReadAdmin { get; set; }

        public int AgentId { get; set; }

        public byte IsPush { get; set; }
    }
}
