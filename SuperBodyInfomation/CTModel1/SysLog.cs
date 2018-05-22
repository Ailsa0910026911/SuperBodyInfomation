namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SysLog")]
    public partial class SysLog
    {
        public long Id { get; set; }

        public byte? MLType { get; set; }

        public int AId { get; set; }

        [StringLength(20)]
        public string TrueName { get; set; }

        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(50)]
        public string ControllerName { get; set; }

        [StringLength(50)]
        public string ActionName { get; set; }

        [Column(TypeName = "text")]
        public string RQData { get; set; }

        [Column(TypeName = "text")]
        public string POSTData { get; set; }

        [StringLength(20)]
        public string IP { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }

        public byte PType { get; set; }
    }
}
