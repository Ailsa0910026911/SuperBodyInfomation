namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TaskTimeSet")]
    public partial class TaskTimeSet
    {
        public int Id { get; set; }

        public int TId { get; set; }

        public DateTime ODate { get; set; }

        public DateTime STime { get; set; }

        public DateTime ETime { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }

        [Column(TypeName = "money")]
        public decimal AllMoney { get; set; }

        [Column(TypeName = "money")]
        public decimal UsedMoney { get; set; }
    }
}
