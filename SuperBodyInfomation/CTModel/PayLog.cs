namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PayLog")]
    public partial class PayLog
    {
        public int Id { get; set; }

        public int PId { get; set; }

        [StringLength(100)]
        public string OId { get; set; }

        [StringLength(100)]
        public string TId { get; set; }

        [Column(TypeName = "money")]
        public decimal? Amount { get; set; }

        [StringLength(20)]
        public string Way { get; set; }

        public DateTime AddTime { get; set; }

        public string Data { get; set; }

        public byte State { get; set; }

        public byte IsDel { get; set; }
    }
}
