namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Equipment")]
    public partial class Equipment
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string No { get; set; }

        [Required]
        [StringLength(64)]
        public string Keys { get; set; }

        [StringLength(64)]
        public string IP { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }

        [StringLength(200)]
        public string IMEI { get; set; }

        public int? RqTimes { get; set; }

        [StringLength(20)]
        public string RqType { get; set; }

        [StringLength(20)]
        public string Mobile { get; set; }

        [StringLength(10)]
        public string IfYY { get; set; }

        [StringLength(20)]
        public string MobiType { get; set; }

        [StringLength(20)]
        public string SysVer { get; set; }

        [StringLength(20)]
        public string SoftVer { get; set; }

        [StringLength(20)]
        public string SignalType { get; set; }
    }
}
