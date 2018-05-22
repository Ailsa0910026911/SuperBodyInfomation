namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("MsgUser")]
    public partial class MsgUser
    {
        public int Id { get; set; }

        public int UId { get; set; }

        [StringLength(100)]
        public string Name { get; set; }

        public string Info { get; set; }

        public byte State { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }

        public string ReadUsers { get; set; }

        public string DeleteUsers { get; set; }

        public int? PId { get; set; }

        public string SendUsers { get; set; }
    }
}
