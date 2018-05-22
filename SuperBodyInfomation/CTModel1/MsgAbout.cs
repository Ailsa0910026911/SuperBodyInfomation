namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("MsgAbout")]
    public partial class MsgAbout
    {
        public int Id { get; set; }

        public int PId { get; set; }

        [StringLength(100)]
        public string Name { get; set; }

        public string Info { get; set; }

        public byte State { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }
    }
}
