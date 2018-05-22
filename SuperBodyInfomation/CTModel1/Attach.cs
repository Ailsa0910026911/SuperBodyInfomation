namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Attach")]
    public partial class Attach
    {
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string AFile { get; set; }

        public byte AType { get; set; }

        public int SAId { get; set; }

        public int AgentId { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }

        public byte State { get; set; }

        public int SLogId { get; set; }

        [StringLength(500)]
        public string UpLoadFile { get; set; }

        public int RemoveSLogId { get; set; }
    }
}
