namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("MsgCallBack")]
    public partial class MsgCallBack
    {
        public int Id { get; set; }

        public int? UId { get; set; }

        [StringLength(20)]
        public string NeekName { get; set; }

        [StringLength(100)]
        public string Linker { get; set; }

        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(2000)]
        public string Info { get; set; }

        [StringLength(2000)]
        public string Result { get; set; }

        public byte State { get; set; }

        public DateTime AddTime { get; set; }

        public DateTime? EditTime { get; set; }

        public int? AId { get; set; }

        public byte IsDel { get; set; }
    }
}
