namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UserFrozenLog")]
    public partial class UserFrozenLog
    {
        public int Id { get; set; }

        [StringLength(50)]
        public string OpName { get; set; }

        public int LogType { get; set; }

        [StringLength(2000)]
        public string Remark { get; set; }

        [StringLength(200)]
        public string Img { get; set; }

        public int UId { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }

        public int? OpType { get; set; }

        public int? AId { get; set; }

        [StringLength(2000)]
        public string InteriorRemark { get; set; }

        [Column(TypeName = "money")]
        public decimal? StopPayMoney { get; set; }

        public byte? Platform { get; set; }
    }
}
