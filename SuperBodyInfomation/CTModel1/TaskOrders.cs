namespace CTModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TaskOrders
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string OId { get; set; }

        public DateTime ODate { get; set; }

        public byte State { get; set; }

        public DateTime AddTime { get; set; }

        public byte IsDel { get; set; }
    }
}
