namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("AdInfo")]
    public partial class AdInfo
    {
        public int Id { get; set; }

        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(300)]
        public string Pic { get; set; }

        [StringLength(300)]
        public string Url { get; set; }

        public int? TId { get; set; }

        [StringLength(10)]
        public string Tag { get; set; }

        public byte ModuleType { get; set; }

        public byte? State { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public int Sort { get; set; }

        public byte IsDel { get; set; }

        public int AgentId { get; set; }
    }
}
