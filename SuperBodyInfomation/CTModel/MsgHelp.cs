namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("MsgHelp")]
    public partial class MsgHelp
    {
        public int Id { get; set; }

        public int PId { get; set; }

        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(200)]
        public string Info { get; set; }

        public byte State { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }

        [StringLength(100)]
        public string Pic1 { get; set; }

        [StringLength(100)]
        public string Pic2 { get; set; }

        [StringLength(100)]
        public string Pic3 { get; set; }

        public int Click { get; set; }
    }
}
