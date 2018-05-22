namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SMSCode")]
    public partial class SMSCode
    {
        public int Id { get; set; }

        public byte CType { get; set; }

        public int UId { get; set; }

        [Required]
        [StringLength(20)]
        public string Mobile { get; set; }

        [Required]
        [StringLength(10)]
        public string Code { get; set; }

        public DateTime AddTime { get; set; }

        public byte State { get; set; }

        public byte IsDel { get; set; }
    }
}
