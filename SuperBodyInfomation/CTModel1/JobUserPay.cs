namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("JobUserPay")]
    public partial class JobUserPay
    {
        public int Id { get; set; }

        public int UId { get; set; }

        public int PayWay { get; set; }

        [StringLength(50)]
        public string MerId { get; set; }

        [StringLength(1000)]
        public string MerKey { get; set; }

        [StringLength(1000)]
        public string ArrayInfo { get; set; }

        public byte MerState { get; set; }

        [StringLength(100)]
        public string MerMsg { get; set; }

        public byte CardState { get; set; }

        [StringLength(100)]
        public string CardMsg { get; set; }

        public byte? BusiState { get; set; }

        [StringLength(100)]
        public string BusiMsg { get; set; }

        public DateTime? AddTime { get; set; }

        public byte IsDel { get; set; }
    }
}
