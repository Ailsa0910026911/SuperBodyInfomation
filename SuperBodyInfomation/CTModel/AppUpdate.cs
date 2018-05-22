namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("AppUpdate")]
    public partial class AppUpdate
    {
        public int Id { get; set; }

        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(50)]
        public string Tag { get; set; }

        [StringLength(50)]
        public string ApkVer { get; set; }

        public int? ApkInt { get; set; }

        [StringLength(200)]
        public string ApkUrl { get; set; }

        [StringLength(200)]
        public string ApkYYB { get; set; }

        public string ApkInfo { get; set; }

        [StringLength(50)]
        public string ApkColor { get; set; }

        public byte APKState { get; set; }

        [StringLength(50)]
        public string IosVer { get; set; }

        public int? IosInt { get; set; }

        [StringLength(200)]
        public string IosUrl { get; set; }

        [StringLength(200)]
        public string IosYYB { get; set; }

        public string IosInfo { get; set; }

        public byte IOSState { get; set; }

        [StringLength(50)]
        public string IosColor { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }
    }
}
