namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UserTrack")]
    public partial class UserTrack
    {
        public int Id { get; set; }

        public int UId { get; set; }

        [StringLength(50)]
        public string UserName { get; set; }

        [StringLength(50)]
        public string ENo { get; set; }

        [StringLength(20)]
        public string OPType { get; set; }

        [StringLength(64)]
        public string IP { get; set; }

        [StringLength(200)]
        public string IPAddress { get; set; }

        [StringLength(20)]
        public string EqMobile { get; set; }

        [StringLength(50)]
        public string MobileAddress { get; set; }

        [StringLength(10)]
        public string IfYY { get; set; }

        [StringLength(20)]
        public string MobiType { get; set; }

        [StringLength(20)]
        public string MobiSysType { get; set; }

        [StringLength(20)]
        public string SysVer { get; set; }

        [StringLength(20)]
        public string SoftVer { get; set; }

        [StringLength(20)]
        public string SignalType { get; set; }

        [StringLength(200)]
        public string GPSAddress { get; set; }

        [StringLength(20)]
        public string GPSX { get; set; }

        [StringLength(20)]
        public string GPSY { get; set; }

        [StringLength(2000)]
        public string Remark { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }
    }
}
