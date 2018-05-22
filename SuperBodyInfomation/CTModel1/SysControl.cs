namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SysControl")]
    public partial class SysControl
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Tag { get; set; }

        [StringLength(200)]
        public string CName { get; set; }

        public byte State { get; set; }

        public DateTime STime { get; set; }

        public DateTime ETime { get; set; }

        public byte TimeType { get; set; }

        public int SNum { get; set; }

        public int ENum { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }

        public int Sort { get; set; }

        public int PayWay { get; set; }

        public byte IsPay { get; set; }

        public byte LagEntryNum { get; set; }

        public byte LagEntryDay { get; set; }
    }
}
