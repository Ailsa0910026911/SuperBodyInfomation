namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TaskCashInfo")]
    public partial class TaskCashInfo
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string OId { get; set; }

        public int? TId { get; set; }

        public byte State { get; set; }

        public byte OState { get; set; }

        public byte NState { get; set; }

        public DateTime AddTime { get; set; }

        public string Remark { get; set; }

        public byte IsDel { get; set; }

        public DateTime? STime { get; set; }

        public DateTime? ETime { get; set; }
    }
}
