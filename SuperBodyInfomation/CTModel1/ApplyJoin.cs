namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ApplyJoin")]
    public partial class ApplyJoin
    {
        public int Id { get; set; }

        public byte ServiceType { get; set; }

        public byte ApplyType { get; set; }

        [StringLength(10)]
        public string Linker { get; set; }

        [StringLength(20)]
        public string Mobile { get; set; }

        [StringLength(20)]
        public string ComName { get; set; }

        public int? Province { get; set; }

        public int? City { get; set; }

        public int? District { get; set; }

        public byte IsDel { get; set; }

        public DateTime AddTime { get; set; }

        public byte State { get; set; }

        public int AgentId { get; set; }

        public int AgentAId { get; set; }

        public string Remark { get; set; }

        public int? TiePaiAgentId { get; set; }

        [StringLength(100)]
        public string TiePaiAgentName { get; set; }

        [StringLength(100)]
        public string AgentName { get; set; }
    }
}
