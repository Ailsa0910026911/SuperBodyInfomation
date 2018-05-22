namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TaskCash")]
    public partial class TaskCash
    {
        public int Id { get; set; }

        public byte State { get; set; }

        public DateTime AddTime { get; set; }

        public DateTime? STime { get; set; }

        public DateTime? ETime { get; set; }

        public int? Total { get; set; }

        public int? Success { get; set; }

        public int? Fail { get; set; }

        public byte IsDel { get; set; }
    }
}
