namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("CheckTask")]
    public partial class CheckTask
    {
        public int Id { get; set; }

        public DateTime TaskDate { get; set; }

        public bool IsComplete { get; set; }

        public byte IsDel { get; set; }

        public DateTime? RunOverTime { get; set; }

        public DateTime RunBeginTime { get; set; }

        public int Progress { get; set; }
    }
}
