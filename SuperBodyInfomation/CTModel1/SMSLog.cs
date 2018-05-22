namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SMSLog")]
    public partial class SMSLog
    {
        public int Id { get; set; }

        public int UId { get; set; }

        public string Mobile { get; set; }

        [StringLength(1000)]
        public string SendText { get; set; }

        public byte State { get; set; }

        public DateTime? Timing { get; set; }

        public DateTime? SendTime { get; set; }

        [StringLength(20)]
        public string Code { get; set; }

        [StringLength(1000)]
        public string Result { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }
    }
}
